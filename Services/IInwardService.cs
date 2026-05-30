using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IInwardService
    {
        Task<int> SaveInwardAsync(InwardCreateDto request);
        Task SaveInwardSizeCountsAsync(int inwardId, string styleNo, string designName, string colour, List<SizeDto> sizes);
        Task<(int InwardId, string Message)> SaveMeterInwardAsync(InwardMeterSaveRequest request);
        Task<IEnumerable<Models.DTOs.SizeResponseDto>> GetSizesByColourStyleAsync(int companyId, string colour, string styleNo);
        Task<IEnumerable<Models.DTOs.MeterResponseDto>> GetMetersByColourStyleAsync(int companyId, string colour, string styleNo);
        Task<Models.DTOs.InwardByDcResponseDto> GetInwardByCompanyAndDcAsync(int companyId, string inwardDcNo);
        Task<string> UpdateInwardAsync(Models.DTOs.InwardUpdateDto inwardUpdate);
        Task<IEnumerable<Models.DTOs.DesignStyleColourDto>> GetDesignStyleColourByCompanyAsync(int companyId);
    }
}
