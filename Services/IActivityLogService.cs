using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IActivityLogService
    {
        Task<ActivityLogResponse> GetActivityLogAsync(ActivityLogRequest request);
        Task<ActivityLogResponse> AdvancedFilterAsync(ActivityLogRequest request);
    }
}
