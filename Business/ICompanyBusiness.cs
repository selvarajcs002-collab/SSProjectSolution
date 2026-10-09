using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Business
{
    public interface ICompanyBusiness
    {
        Task<CommonResponse> SaveCompany(CompanyRequest request);
        Task<IEnumerable<KeyValueModel>> GetCompanyList();
        Task<IEnumerable<CompanySearchResult>> SearchCompanies(string? query, int limit = 25);
        Task<CompanyModel> GetCompanyById(int companyId);
    }
}
