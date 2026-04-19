using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Repositories
{
    public interface IInwardRepository
    {
        Task<IEnumerable<SizeResponseDto>> GetSizesByColourStyleAsync(int companyId, string colour, string styleNo);
        Task<InwardByDcResponseDto> GetInwardByCompanyAndDcAsync(int companyId, string inwardDcNo);
        Task<string> UpdateInwardAsync(InwardUpdateDto inwardUpdate);
        Task<IEnumerable<DesignStyleColourDto>> GetDesignStyleColourByCompanyAsync(int companyId);
    }
}
