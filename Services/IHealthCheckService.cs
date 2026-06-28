using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IHealthCheckService
    {
        Task<(bool IsHealthy, string Message)> CheckHealthAsync();
    }
}
