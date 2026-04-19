using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Business
{
    public interface ICompanyBusiness
    {
        Task<CommonResponse> SaveCompany(CompanyRequest request);
        Task<IEnumerable<KeyValueModel>> GetCompanyList();
        Task<CompanyModel> GetCompanyById(int companyId);
    }
}
