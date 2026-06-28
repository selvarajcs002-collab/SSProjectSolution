using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSProjectSolution.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class PrintService : IPrintService
    {
        private readonly PrintSettings _settings;
        private readonly ILogger<PrintService> _logger;

        public PrintService(IOptionsSnapshot<PrintSettings> options, ILogger<PrintService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<bool> PrintPdfAsync(string filePath, string overridePrinterName = null)
        {
            if (!_settings.EnablePrinting)
            {
                _logger.LogInformation("Printing is disabled in configuration.");
                return false;
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file to print was not found: {filePath}");
            }

            if (!_settings.UseSumatraPDF)
            {
                throw new NotSupportedException("Currently, only SumatraPDF is supported as the print engine in this implementation.");
            }

            return await Task.Run(() => PrintWithSumatraPDF(filePath, overridePrinterName));
        }

        private bool PrintWithSumatraPDF(string filePath, string overridePrinterName)
        {
            string printerName = !string.IsNullOrWhiteSpace(overridePrinterName) ? overridePrinterName : _settings.PrinterName;
            int copies = _settings.Copies;
            int timeout = _settings.PrintTimeout;

            if (string.IsNullOrWhiteSpace(printerName))
            {
                throw new InvalidOperationException("Printer name is not configured.");
            }

            string sumatraPath = "SumatraPDF.exe";

            // Build print settings string
            var printSettingsList = new System.Collections.Generic.List<string>();
            printSettingsList.Add($"{copies}x");

            if (!string.IsNullOrWhiteSpace(_settings.PaperSize) && !_settings.PaperSize.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                printSettingsList.Add($"paper={_settings.PaperSize}");
            }

            if (_settings.PrintOrientation.Equals("Landscape", StringComparison.OrdinalIgnoreCase))
            {
                printSettingsList.Add("landscape");
            }
            else if (_settings.PrintOrientation.Equals("Portrait", StringComparison.OrdinalIgnoreCase))
            {
                printSettingsList.Add("portrait");
            }

            // Prevent SumatraPDF from shrinking the document by forcing noscale
            printSettingsList.Add("noscale"); 

            string printSettingsStr = string.Join(",", printSettingsList);

            string arguments = $"-print-to \"{printerName}\" -print-settings \"{printSettingsStr}\" -silent \"{filePath}\"";

            _logger.LogInformation("Executing Print Engine: {Engine} {Arguments}", sumatraPath, arguments);

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = sumatraPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                
                try
                {
                    process.Start();

                    bool exited = process.WaitForExit(timeout);

                    if (!exited)
                    {
                        _logger.LogError("Print process timed out after {Timeout}ms.", timeout);
                        process.Kill();
                        throw new TimeoutException($"Print engine timed out after {timeout}ms.");
                    }

                    if (process.ExitCode != 0)
                    {
                        _logger.LogError("Print engine exited with code {ExitCode}", process.ExitCode);
                        throw new Exception($"Print engine failed with exit code {process.ExitCode}");
                    }

                    _logger.LogInformation("Print job submitted successfully to {PrinterName}.", printerName);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to execute print process.");
                    throw;
                }
            }
        }
    }
}
