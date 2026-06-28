using Microsoft.Extensions.Options;
using SSProjectSolution.Settings;
using SSProjectSolution.Validators;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class HealthCheckService : IHealthCheckService
    {
        private readonly IFileValidator _fileValidator;
        private readonly IPrinterValidator _printerValidator;
        private readonly PrintSettings _settings;

        public HealthCheckService(
            IFileValidator fileValidator,
            IPrinterValidator printerValidator,
            IOptionsSnapshot<PrintSettings> options)
        {
            _fileValidator = fileValidator;
            _printerValidator = printerValidator;
            _settings = options.Value;
        }

        public async Task<(bool IsHealthy, string Message)> CheckHealthAsync()
        {
            if (!_settings.EnableHealthCheck)
            {
                return (true, "Health check is disabled in configuration.");
            }

            return await Task.Run(() =>
            {
                if (_settings.EnableSaving)
                {
                    var folderResult = _fileValidator.ValidateFolder(_settings.GetAbsoluteSaveFolder());
                    if (!folderResult.IsValid)
                    {
                        return (false, $"Folder Health Error: {folderResult.ErrorMessage}");
                    }
                }

                if (_settings.EnablePrinting)
                {
                    var printerResult = _printerValidator.ValidatePrinter(_settings.PrinterName);
                    if (!printerResult.IsValid)
                    {
                        return (false, $"Printer Health Error: {printerResult.ErrorMessage}");
                    }
                }

                return (true, "Healthy. Printer and Folder access verified.");
            });
        }
    }
}
