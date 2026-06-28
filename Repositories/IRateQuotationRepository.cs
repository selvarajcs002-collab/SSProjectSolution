using SSProjectSolution.Models;

namespace SSProjectSolution.Repositories
{
    public interface IRateQuotationRepository
    {
        Task<(long NewId, int StatusCode, string StatusMessage)> CreateAsync(RateQuotationEntity entity);
        Task<(int StatusCode, string StatusMessage)> UpdateAsync(RateQuotationEntity entity);
        Task<(int StatusCode, string StatusMessage)> DeleteAsync(long id, long modifiedBy);
        Task<(RateQuotationEntity? Entity, int StatusCode, string StatusMessage)> GetByIdAsync(long id);
        Task<(IEnumerable<RateQuotationEntity> Entities, int StatusCode, string StatusMessage)> GetAllAsync();
        Task<(IEnumerable<RateQuotationEntity> Entities, int StatusCode, string StatusMessage)> SearchAsync(Models.DTOs.RateQuotationSearchDto searchDto);
        Task<(IEnumerable<RateQuotationEntity> Entities, int TotalRecords, int StatusCode, string StatusMessage)> GetPagedAsync(Models.DTOs.RateQuotationSearchDto searchDto);
    }
}
