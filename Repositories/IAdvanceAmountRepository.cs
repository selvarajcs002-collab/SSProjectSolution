using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public interface IAdvanceAmountRepository
    {
        Task<IEnumerable<AdvanceAmountDto>> GetAllAdvancesAsync();
        Task<AdvanceAmountDto> GetAdvanceByIdAsync(int id);
        Task<int> AddAdvanceAsync(AddAdvanceAmountDto dto);
        Task<bool> UpdateAdvanceAsync(UpdateAdvanceAmountDto dto);
        Task<bool> DeleteAdvanceAsync(int id);
        Task<IEnumerable<AdvanceAmountDto>> GetAdvancesForExportAsync(DateTime fromDate, DateTime toDate);
        Task<bool> UpdateAdvanceChallanAsync(int id, string challanNo, string challanFileName);
        Task<string> AllocateNextChallanNoAsync(int advanceId, DateTime advanceDate);
    }
}
