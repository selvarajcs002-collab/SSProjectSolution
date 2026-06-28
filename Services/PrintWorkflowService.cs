using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SSProjectSolution.Documents;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

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
        private readonly IPrinterHealthService _printerHealthService;
        private readonly IPrintService _printService;
        private readonly ILogger<PrintWorkflowService> _logger;

        public PrintWorkflowService(
            IConfiguration configuration,
            IOptionsSnapshot<PrintSettings> options,
            IPrinterHealthService printerHealthService,
            IPrintService printService,
            ILogger<PrintWorkflowService> logger)
        {
            _configuration = configuration;
            _settings = options.Value;
            _printerHealthService = printerHealthService;
            _printService = printService;
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

                // File naming format from settings, e.g., "DC_{0}_{1}.pdf"
                string format = !string.IsNullOrWhiteSpace(_settings.FileNamingFormat) ? _settings.FileNamingFormat : "DC_{0}_{1}.pdf";
                string timestamp = _settings.AppendTimestamp ? DateTime.Now.ToString("yyyyMMddHHmmss") : "";
                string fileName = string.Format(format, request.DcNo, timestamp);
                filePath = Path.Combine(saveFolder, fileName);
                
                response.SavedFilePath = filePath;
                response.PrinterName = !string.IsNullOrWhiteSpace(request.PrinterName) ? request.PrinterName : _settings.PrinterName;

                // 2. Generate PDF
                _logger.LogInformation("Generating PDF for DCNo: {DcNo}", request.DcNo);
                var document = new DeliveryChallanDocument(request, _configuration);
                document.GeneratePdf(filePath);

                // Validate PDF
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

            // 3. Print Health Check
            if (_settings.EnableHealthCheck)
            {
                var healthStatus = await _printerHealthService.CheckPrinterStatusAsync(response.PrinterName);
                if (!healthStatus.IsSuccess)
                {
                    _logger.LogWarning("Printer health check failed: {ErrorCode} - {ErrorDescription}", healthStatus.ErrorCode, healthStatus.ErrorDescription);
                    response.PrintSuccess = false;
                    response.Message = $"Delivery Challan PDF was saved successfully. File Location: {filePath}\nPrinting failed. Please verify that the printer is connected, online, has paper, and is not paused.";
                    response.ErrorCode = healthStatus.ErrorCode;
                    response.ErrorDescription = healthStatus.ErrorDescription;
                    
                    // specific messages based on error code
                    if (healthStatus.ErrorCode == "PRINTER_NOT_INSTALLED")
                        response.Message = "Delivery Challan PDF was saved successfully. No printer is installed.";
                    else if (healthStatus.ErrorCode == "PRINTER_OFFLINE")
                        response.Message = "Delivery Challan PDF was saved successfully. The printer is offline.";
                    else if (healthStatus.ErrorCode == "PRINTER_PAUSED")
                        response.Message = "Delivery Challan PDF was saved successfully. The printer is currently paused.";
                    else if (healthStatus.ErrorCode == "OUT_OF_PAPER")
                        response.Message = "Delivery Challan PDF was saved successfully. The printer is out of paper.";
                        
                    await InsertPrintLogAsync(request, filePath, false);
                    return response;
                }
            }

            // 4. Print
            _logger.LogInformation("Sending PDF to Printer {PrinterName}", response.PrinterName);
            try
            {
                _logger.LogInformation("Waiting for Printer Completion");
                bool printStatus = await _printService.PrintPdfAsync(filePath, response.PrinterName);
                
                response.PrintSuccess = printStatus;
                if (printStatus)
                {
                    _logger.LogInformation("Printer Success");
                    response.Message = "Delivery Challan printed successfully.";
                    response.ErrorCode = "";
                }
                else
                {
                    _logger.LogWarning("Printer Failed (PrintService returned false)");
                    response.Message = $"Delivery Challan PDF was saved successfully. File Location: {filePath}\nPrinting failed. Please verify that the printer is connected, online, has paper, and is not paused.";
                    response.ErrorCode = "PRINT_FAILED";
                }
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "Printer timeout");
                response.PrintSuccess = false;
                response.Message = "Delivery Challan PDF was saved successfully. The printer did not respond within the configured timeout.";
                response.ErrorCode = "PRINTER_TIMEOUT";
                response.ErrorDescription = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown printer error");
                response.PrintSuccess = false;
                response.Message = "Delivery Challan PDF was saved successfully. An unexpected printer error occurred.";
                response.ErrorCode = "UNKNOWN_ERROR";
                response.ErrorDescription = ex.Message;
            }

            // 5. Log
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
