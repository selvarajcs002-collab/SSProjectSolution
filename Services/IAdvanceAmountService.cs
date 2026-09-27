using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IAdvanceAmountService
    {
        Task<IEnumerable<AdvanceAmountDto>> GetAllAdvancesAsync();
        Task<AdvanceAmountDto> GetAdvanceByIdAsync(int id);
        Task<(int Id, string ChallanNo, string ChallanFileName)> AddAdvanceAsync(AddAdvanceAmountDto dto);
        Task<(bool Success, string ChallanNo, string ChallanFileName)> UpdateAdvanceAsync(UpdateAdvanceAmountDto dto);
        Task<bool> DeleteAdvanceAsync(int id);
        Task<byte[]> GenerateExportExcelAsync(DateTime fromDate, DateTime toDate, string generatedBy);
        Task<AdvanceChallanFileResult> GenerateChallanAsync(GenerateAdvanceChallanRequest request, string generatedBy);
    }
}
