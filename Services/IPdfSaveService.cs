using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IPdfSaveService
    {
        Task<string> SavePdfAsync(byte[] pdfBytes, string dcNumber);
    }
}
