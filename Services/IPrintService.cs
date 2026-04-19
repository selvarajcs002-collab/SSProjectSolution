using System.Threading.Tasks;
using SSProjectSolution.Request;

namespace SSProjectSolution.Services
{
    public interface IPrintService
    {
        Task<string> SavePdfAsync(PrintPdfRequest request);
    }
}
