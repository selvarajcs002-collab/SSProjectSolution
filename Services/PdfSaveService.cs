using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSProjectSolution.Settings;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class PdfSaveService : IPdfSaveService
    {
        private readonly PrintSettings _settings;
        private readonly ILogger<PdfSaveService> _logger;

        public PdfSaveService(IOptionsSnapshot<PrintSettings> options, ILogger<PdfSaveService> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> SavePdfAsync(byte[] pdfBytes, string dcNumber)
        {
            if (!_settings.EnableSaving)
            {
                _logger.LogInformation("Saving is disabled in configuration.");
                return string.Empty;
            }

            string saveFolder = _settings.GetAbsoluteSaveFolder();

            if (string.IsNullOrWhiteSpace(saveFolder))
            {
                throw new InvalidOperationException("Save folder path is not configured.");
            }

            if (!Directory.Exists(saveFolder))
            {
                if (_settings.CreateFolderAutomatically)
                {
                    _logger.LogInformation("Creating directory: {SaveFolder}", saveFolder);
                    Directory.CreateDirectory(saveFolder);
                }
                else
                {
                    throw new DirectoryNotFoundException($"Save folder '{saveFolder}' does not exist and automatic creation is disabled.");
                }
            }

            string safeDcNumber = string.Join("_", dcNumber.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safeDcNumber}.pdf";
            string filePath = Path.Combine(saveFolder, fileName);

            if (File.Exists(filePath))
            {
                if (_settings.OverwriteExistingFile)
                {
                    _logger.LogWarning("File {FilePath} already exists. Overwriting.", filePath);
                }
                else if (_settings.AppendTimestamp)
                {
                    fileName = $"{safeDcNumber}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                    filePath = Path.Combine(saveFolder, fileName);
                    _logger.LogInformation("File existed. Appended timestamp. New file path: {FilePath}", filePath);
                }
                else
                {
                    throw new IOException($"File '{filePath}' already exists and overwrite/append options are disabled.");
                }
            }

            _logger.LogInformation("Saving PDF to {FilePath}", filePath);
            
            // Async save
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await fileStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
            }

            _logger.LogInformation("File saved successfully.");
            return filePath;
        }
    }
}
