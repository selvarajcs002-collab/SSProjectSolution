using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IRateQuotationService
    {
        Task<ApiResponse<long>> CreateAsync(RateQuotationCreateDto createDto);
        Task<ApiResponse<bool>> UpdateAsync(long id, RateQuotationUpdateDto updateDto);
        Task<ApiResponse<bool>> DeleteAsync(long id, long modifiedBy);
        Task<ApiResponse<RateQuotationResponseDto>> GetByIdAsync(long id);
        Task<ApiResponse<IEnumerable<RateQuotationResponseDto>>> GetAllAsync();
        Task<ApiResponse<IEnumerable<RateQuotationResponseDto>>> SearchAsync(RateQuotationSearchDto searchDto);
        Task<ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>> GetPagedAsync(RateQuotationSearchDto searchDto);
    }
}
