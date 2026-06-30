using SSProjectSolution.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public interface IPrintJobRepository
    {
        Task<string> CreateJobAsync(PrintJob job);
        Task<PrintJob?> GetJobByIdAsync(string jobId);
        Task<IEnumerable<PrintJob>> GetPendingJobsByUserIdAsync(string userId);
        Task<bool> UpdateJobStatusAsync(string jobId, string status, string? failureReason = null);
        Task<bool> MarkJobAsDownloadedAsync(string jobId);
        Task<bool> IncrementRetryCountAsync(string jobId);
    }
}
