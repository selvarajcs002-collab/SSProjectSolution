using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SSProjectSolution.Documents;
using SSProjectSolution.Models;
using SSProjectSolution.Repositories;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Settings;
using SSProjectSolution.SignalR;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IPrintWorkflowService
    {
        Task<PrintChallanResponse> GenerateAndPrintDcAsync(GenerateDcRequest request);
    }

    public class PrintWorkflowService : IPrintWorkflowService
    {
        private readonly IConfiguration _configuration;
        private readonly PrintSettings _settings;
        private readonly IPrintJobRepository _printJobRepository;
        private readonly IHubContext<PrintHub> _hubContext;
        private readonly ILogger<PrintWorkflowService> _logger;

        public PrintWorkflowService(
            IConfiguration configuration,
            IOptionsSnapshot<PrintSettings> options,
            IPrintJobRepository printJobRepository,
            IHubContext<PrintHub> hubContext,
            ILogger<PrintWorkflowService> logger)
        {
            _configuration = configuration;
            _settings = options.Value;
            _printJobRepository = printJobRepository;
            _hubContext = hubContext;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<PrintChallanResponse> GenerateAndPrintDcAsync(GenerateDcRequest request)
        {
            var response = new PrintChallanResponse();
            string filePath = string.Empty;
            
            _logger.LogInformation("Print Challan Started for DCNo: {DcNo}", request.DcNo);

            try
            {
                // 1. Determine Save Path
                string saveFolder = _settings.GetAbsoluteSaveFolder();
                if (string.IsNullOrWhiteSpace(saveFolder))
                {
                    saveFolder = @"C:\SSManagement\DeliveryChallan";
                }

                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                string format = !string.IsNullOrWhiteSpace(_settings.FileNamingFormat) ? _settings.FileNamingFormat : "DC_{0}_{1}.pdf";
                string timestamp = _settings.AppendTimestamp ? DateTime.Now.ToString("yyyyMMddHHmmss") : "";
                string fileName = string.Format(format, request.DcNo, timestamp);
                filePath = Path.Combine(saveFolder, fileName);
                
                response.SavedFilePath = filePath;
                
                // For agent-based printing, we use the agent's config for printer name, so we don't strictly need it here,
                // but we keep it in response if the UI uses it.
                response.PrinterName = !string.IsNullOrWhiteSpace(request.PrinterName) ? request.PrinterName : _settings.PrinterName;

                // 2. Generate PDF
                _logger.LogInformation("Generating PDF for DCNo: {DcNo}", request.DcNo);
                var document = new DeliveryChallanDocument(request, _configuration);
                document.GeneratePdf(filePath);

                if (!File.Exists(filePath))
                {
                    throw new Exception("PDF generation failed: File does not exist.");
                }

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    throw new Exception("PDF generation failed: File size is 0 bytes.");
                }

                response.PdfGenerated = true;
                response.PdfSaved = true;
                _logger.LogInformation("PDF Generated Successfully and Saved to {FilePath}", filePath);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to generate Delivery Challan PDF.");
                response.PdfGenerated = false;
                response.PdfSaved = false;
                response.PrintSuccess = false;
                response.Message = "Unable to generate or save Delivery Challan PDF. Printing was not started.";
                response.ErrorCode = "PDF_GENERATION_FAILED";
                response.ErrorDescription = ex.Message;
                return response;
            }

            // 3. Create Print Job & Notify Print Agent
            try
            {
                var printJob = new PrintJob
                {
                    JobId = Guid.NewGuid().ToString(),
                    DocumentType = "DeliveryChallan",
                    DocumentNumber = request.DcNo,
                    PdfPath = filePath,
                    PrinterName = request.PrinterName,
                    Copies = 3, // Client requested exactly 3 copies
                    UserId = request.PrintedBy, // We map PrintedBy to AgentId/UserId
                    Status = "Queued",
                    CreatedDate = DateTime.UtcNow
                };

                await _printJobRepository.CreateJobAsync(printJob);

                var agentConnectionId = PrintHub.GetConnectionIdForUser(printJob.UserId);
                if (!string.IsNullOrEmpty(agentConnectionId))
                {
                    // Print Agent is online, notify immediately
                    _logger.LogInformation("Notifying Print Agent (ConnectionId: {ConnectionId}) for Job {JobId}", agentConnectionId, printJob.JobId);
                    
                    // We map PrintJob to a simple DTO to send to the client (matches PrintJobDto on agent)
                    var jobDto = new
                    {
                        jobId = printJob.JobId,
                        documentType = printJob.DocumentType,
                        documentNumber = printJob.DocumentNumber,
                        printerName = printJob.PrinterName,
                        copies = printJob.Copies,
                        paperSize = printJob.PaperSize,
                        orientation = printJob.Orientation
                    };

                    await _hubContext.Clients.Client(agentConnectionId).SendAsync("ReceivePrintJob", jobDto);
                    
                    response.Message = "Delivery Challan queued and sent to Print Agent successfully.";
                }
                else
                {
                    // Print Agent is offline, job remains "Queued" in DB
                    _logger.LogInformation("Print Agent for user {UserId} is offline. Job {JobId} is queued.", printJob.UserId, printJob.JobId);
                    response.Message = "Delivery Challan queued successfully. It will print when the Print Agent comes online.";
                }

                response.PrintSuccess = true; // Queued successfully
                response.ErrorCode = "";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create print job or notify agent.");
                response.PrintSuccess = false;
                response.Message = "Delivery Challan PDF was saved successfully, but failed to queue for printing.";
                response.ErrorCode = "QUEUE_FAILED";
                response.ErrorDescription = ex.Message;
            }

            await InsertPrintLogAsync(request, filePath, response.PrintSuccess);

            return response;
        }

        private async Task InsertPrintLogAsync(GenerateDcRequest request, string pdfPath, bool printStatus)
        {
            await Task.CompletedTask;
            Console.WriteLine($"LOGGED: DCNo={request.DcNo}, PrintedBy={request.PrintedBy}, Path={pdfPath}, Status={printStatus}");
        }
    }
}

