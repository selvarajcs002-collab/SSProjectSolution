using System;
using System.Drawing.Printing;
using System.Linq;

namespace SSProjectSolution.Validators
{
    public class PrinterValidator : IPrinterValidator
    {
        public (bool IsValid, string ErrorMessage) ValidatePrinter(string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
            {
                return (false, "Printer name cannot be empty in configuration.");
            }

            try
            {
                // In a Windows Server environment, PrinterSettings.InstalledPrinters is the standard way to check
                bool printerExists = PrinterSettings.InstalledPrinters.Cast<string>()
                    .Any(p => p.Equals(printerName, StringComparison.OrdinalIgnoreCase));

                if (!printerExists)
                {
                    return (false, $"Printer '{printerName}' is not installed or accessible by the server.");
                }

                // Check basic status
                PrinterSettings settings = new PrinterSettings { PrinterName = printerName };
                if (!settings.IsValid)
                {
                    return (false, $"Printer '{printerName}' is not in a valid state.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to validate printer: {ex.Message}");
            }
        }
    }
}
