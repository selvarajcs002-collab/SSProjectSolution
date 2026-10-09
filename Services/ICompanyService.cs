using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface ICompanyService
    {
        Task<CommonResponse> ManageCompanyAsync(CompanyRequest request);
        Task<IEnumerable<KeyValueModel>> GetCompanyListAsync();
        Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string? query, int limit = 25);
        Task<CompanyModel> GetCompanyByIdAsync(int companyId);
    }
}
