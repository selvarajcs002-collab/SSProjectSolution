using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IPrintService
    {
        Task<bool> PrintPdfAsync(string filePath, string overridePrinterName = null);
    }
}
