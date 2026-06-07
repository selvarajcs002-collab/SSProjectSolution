using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IStatusFilterService
    {
        Task<StatusFilterResponse> SearchAsync(StatusFilterRequest request);
    }
}
