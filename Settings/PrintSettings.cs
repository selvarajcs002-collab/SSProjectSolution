using System.IO;

namespace SSProjectSolution.Settings
{
    /// <summary>
    /// Configuration section for the Print Module, read from appsettings.json
    /// </summary>
    public class PrintSettings
    {
        // Save Settings
        public string SaveFolder { get; set; } = string.Empty;
        public bool EnableSaving { get; set; } = true;
        public bool CreateFolderAutomatically { get; set; } = true;
        public bool OverwriteExistingFile { get; set; } = false;
        public bool AppendTimestamp { get; set; } = true;
        public string FileNamingFormat { get; set; } = "DC_{0}_{1}.pdf";
        public int MaxFileNameLength { get; set; } = 255;
        public string TempFolder { get; set; } = string.Empty;
        public string WorkingFolder { get; set; } = string.Empty;
        
        // Print Settings
        public bool EnablePrinting { get; set; } = true;
        public string PrinterName { get; set; } = string.Empty;
        public int Copies { get; set; } = 3;
        public string PrintEngine { get; set; } = "SumatraPDF";
        public bool UseSumatraPDF { get; set; } = true;
        public bool UsePdfium { get; set; } = false;
        public bool UseAdobe { get; set; } = false;
        public bool UseGhostScript { get; set; } = false;
        public string PrintOrientation { get; set; } = "Portrait";
        public string PaperSize { get; set; } = "A4";
        public string Margins { get; set; } = "None"; // Or values like "10,10,10,10"
        
        // Timeout & Retry Settings
        public int PrintTimeout { get; set; } = 30000; // ms
        public int PdfGenerationTimeout { get; set; } = 30000; // ms
        public int RetryCount { get; set; } = 1;
        public int RetryDelay { get; set; } = 2000; // ms
        
        // Validation Settings
        public bool EnableHealthCheck { get; set; } = true;
        public bool PrinterValidation { get; set; } = true;
        public bool FolderValidation { get; set; } = true;
        
        // Archive Settings
        public bool DeleteOldFiles { get; set; } = false;
        public int DaysToKeepFiles { get; set; } = 30;
        public bool EnableArchive { get; set; } = false;
        public string ArchiveFolder { get; set; } = string.Empty;
        
        // Other Settings
        public bool EnableLogging { get; set; } = true;
        public bool EnableWatermark { get; set; } = false;
        public string CompanyName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string Environment { get; set; } = "Production";

        // Helper to get absolute path
        public string GetAbsoluteSaveFolder()
        {
            if (string.IsNullOrWhiteSpace(SaveFolder))
                return string.Empty;

            if (Path.IsPathRooted(SaveFolder))
                return SaveFolder;

            return Path.Combine(Directory.GetCurrentDirectory(), SaveFolder);
        }
    }
}
