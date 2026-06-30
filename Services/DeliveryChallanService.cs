using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SSProjectSolution.Response;
using SSProjectSolution.Settings;
using SSProjectSolution.Validators;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SSProjectSolution.Models;
using SSProjectSolution.Repositories;
using Microsoft.AspNetCore.SignalR;
using SSProjectSolution.SignalR;
using Microsoft.AspNetCore.Http;

namespace SSProjectSolution.Services
{
    public class DeliveryChallanService : IDeliveryChallanService
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IPdfSaveService _pdfSaveService;
        private readonly IPrintService _printService;
        private readonly IFileValidator _fileValidator;
        private readonly IPrinterValidator _printerValidator;
        private readonly PrintSettings _settings;
        private readonly ILogger<DeliveryChallanService> _logger;
        private readonly IPrintJobRepository _printJobRepo;
        private readonly IHubContext<PrintHub> _printHub;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeliveryChallanService(
            IPdfGenerator pdfGenerator,
            IPdfSaveService pdfSaveService,
            IPrintService printService,
            IFileValidator fileValidator,
            IPrinterValidator printerValidator,
            IOptionsSnapshot<PrintSettings> options,
            ILogger<DeliveryChallanService> logger,
            IPrintJobRepository printJobRepo,
            IHubContext<PrintHub> printHub,
            IHttpContextAccessor httpContextAccessor)
        {
            _pdfGenerator = pdfGenerator;
            _pdfSaveService = pdfSaveService;
            _printService = printService;
            _fileValidator = fileValidator;
            _printerValidator = printerValidator;
            _settings = options.Value;
            _logger = logger;
            _printJobRepo = printJobRepo;
            _printHub = printHub;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SaveAndPrintResponse> ProcessSaveAndPrintAsync(JObject payload)
        {
            var stopwatch = Stopwatch.StartNew();
            string correlationId = Guid.NewGuid().ToString("N");
            
            _logger.LogInformation("Request Started [CorrelationId: {CorrelationId}]", correlationId);

            var response = new SaveAndPrintResponse
            {
                Success = false,
                CorrelationId = correlationId,
                Copies = _settings.Copies,
                Printer = _settings.PrinterName
            };

            try
            {
                // 1. Initial Validations
                if (payload == null)
                {
                    throw new ArgumentException("Payload cannot be null");
                }

                string dcNumber = payload.Value<string>("dcNo") ?? payload.Value<string>("DcNo");
                if (string.IsNullOrWhiteSpace(dcNumber))
                {
                    // Fallback to generating a unique name if DC number is not found
                    dcNumber = $"DC_{DateTime.Now:yyyyMMddHHmmss}";
                }

                if (_settings.FolderValidation && _settings.EnableSaving)
                {
                    var folderResult = _fileValidator.ValidateFolder(_settings.GetAbsoluteSaveFolder());
                    if (!folderResult.IsValid)
                    {
                        throw new InvalidOperationException($"Folder validation failed: {folderResult.ErrorMessage}");
                    }
                }

                string activePrinter = _settings.PrinterName;
                if (_settings.PrinterValidation && _settings.EnablePrinting)
                {
                    if (string.IsNullOrWhiteSpace(_settings.PrinterName))
                    {
                        throw new InvalidOperationException("Printer validation failed: Printer name is not configured.");
                    }

                    var printers = _settings.PrinterName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    bool anyValid = false;

                    foreach (var p in printers)
                    {
                        var pName = p.Trim();
                        var printerResult = _printerValidator.ValidatePrinter(pName);
                        if (printerResult.IsValid)
                        {
                            activePrinter = pName;
                            anyValid = true;
                            _logger.LogInformation("Selected valid printer: {PrinterName} [CorrelationId: {CorrelationId}]", activePrinter, correlationId);
                            break;
                        }
                        else
                        {
                            _logger.LogWarning("Printer fallback check: {PrinterName} is invalid ({ErrorMessage}) [CorrelationId: {CorrelationId}]", pName, printerResult.ErrorMessage, correlationId);
                        }
                    }

                    if (!anyValid)
                    {
                        throw new InvalidOperationException($"Printer validation failed: None of the configured printers are available.");
                    }
                }
                else if (_settings.EnablePrinting && !string.IsNullOrWhiteSpace(_settings.PrinterName))
                {
                    // If no validation, just take the first one
                    activePrinter = _settings.PrinterName.Split(',')[0].Trim();
                }

                // Update response to reflect the dynamically chosen printer
                response.Printer = activePrinter;

                // 2. Generate PDF
                _logger.LogInformation("PDF Generation Started [CorrelationId: {CorrelationId}]", correlationId);
                byte[] pdfBytes = await ExecuteWithRetryAsync(() => _pdfGenerator.GeneratePdfAsync(payload), 1, 0); // Generation usually shouldn't be retried
                _logger.LogInformation("PDF Generated Successfully [CorrelationId: {CorrelationId}]", correlationId);

                // 3. Save PDF
                string savedPath = string.Empty;
                if (_settings.EnableSaving)
                {
                    _logger.LogInformation("PDF Save Started [CorrelationId: {CorrelationId}]", correlationId);
                    savedPath = await ExecuteWithRetryAsync(() => _pdfSaveService.SavePdfAsync(pdfBytes, dcNumber), _settings.RetryCount, _settings.RetryDelay);
                    response.SavedFilePath = savedPath;
                    _logger.LogInformation("File Saved at {SavedPath} [CorrelationId: {CorrelationId}]", savedPath, correlationId);
                }

                // 4. Print PDF (Delegate to Print Agent)
                if (_settings.EnablePrinting && !string.IsNullOrEmpty(savedPath))
                {
                    _logger.LogInformation("Creating PrintJob and notifying Print Agent [CorrelationId: {CorrelationId}]", correlationId);
                    
                    var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "system";
                    
                    var job = new PrintJob
                    {
                        JobId = Guid.NewGuid().ToString("N"),
                        DocumentType = "DeliveryChallan",
                        DocumentNumber = dcNumber,
                        PdfPath = savedPath,
                        PrinterName = activePrinter,
                        Copies = _settings.Copies,
                        PaperSize = _settings.PaperSize,
                        Orientation = _settings.PrintOrientation,
                        Status = "Queued",
                        UserId = userId,
                        CompanyId = payload.Value<int?>("companyId") // Assuming payload has companyId
                    };
                    
                    await _printJobRepo.CreateJobAsync(job);
                    
                    // Notify SignalR Client
                    var connectionId = PrintHub.GetConnectionIdForUser(userId);
                    if (!string.IsNullOrEmpty(connectionId))
                    {
                        await _printHub.Clients.Client(connectionId).SendAsync("ReceivePrintJob", job);
                        job.Status = "Sent";
                        await _printJobRepo.UpdateJobStatusAsync(job.JobId, "Sent");
                        _logger.LogInformation("Print job sent to agent. JobId: {JobId}, ConnectionId: {ConnectionId}", job.JobId, connectionId);
                    }
                    else
                    {
                        _logger.LogWarning("No active Print Agent connected for UserId: {UserId}. Job queued.", userId);
                    }
                    
                    response.Printed = true; // Set true here to indicate success queuing to the frontend
                }

                // Success completion
                response.Success = true;
                response.Message = "Delivery Challan processed successfully.";
                _logger.LogInformation("Request Completed Successfully [CorrelationId: {CorrelationId}]", correlationId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
                response.ErrorCode = ex.GetType().Name;
                
                _logger.LogError(ex, "Request Failed [CorrelationId: {CorrelationId}] - {ErrorMessage}", correlationId, ex.Message);
            }
            finally
            {
                stopwatch.Stop();
                response.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                _logger.LogInformation("Total Execution Time: {ExecutionTimeMs}ms [CorrelationId: {CorrelationId}]", response.ExecutionTimeMs, correlationId);
            }

            return response;
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action, int maxRetries, int delayMs)
        {
            int attempt = 0;
            while (true)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsTransient(ex) && attempt < maxRetries)
                {
                    attempt++;
                    _logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed due to transient error. Retrying in {Delay}ms.", attempt, maxRetries, delayMs);
                    await Task.Delay(delayMs);
                }
            }
        }

        private bool IsTransient(Exception ex)
        {
            // Typical transient exceptions for I/O and Network/Printer
            return ex is System.IO.IOException ||
                   ex is TimeoutException ||
                   ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   ex.Message.Contains("busy", StringComparison.OrdinalIgnoreCase);
        }
    }
}
