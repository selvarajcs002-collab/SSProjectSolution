using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface ICompanyService
    {
        Task<CommonResponse> ManageCompanyAsync(CompanyRequest request);
        Task<IEnumerable<KeyValueModel>> GetCompanyListAsync();
        Task<CompanyModel> GetCompanyByIdAsync(int companyId);
    }
}
