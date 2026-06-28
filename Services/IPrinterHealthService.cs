using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IPrinterHealthService
    {
        Task<(bool IsSuccess, string ErrorCode, string ErrorDescription)> CheckPrinterStatusAsync(string printerName);
    }
}
