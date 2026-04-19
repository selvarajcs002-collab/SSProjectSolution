using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IDcFilterService
    {
        Task<InwardOutwardNestedResponse> GetInwardOutwardDetailsAsync(InwardOutwardFilterRequest request);
        Task<InwardOutwardNestedResponse> GetInwardOutwardAsync(string mode, int pageNumber = 1, int pageSize = 10);
    }
}
