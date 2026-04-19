using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Services;

namespace SSProjectSolution.Business
{
    public class CompanyBusiness : ICompanyBusiness
    {
        private readonly ICompanyService _companyService;

        public CompanyBusiness(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public async Task<CommonResponse> SaveCompany(CompanyRequest request)
        {
            // Add business logic/validation as needed
            return await _companyService.ManageCompanyAsync(request);
        }

        public async Task<IEnumerable<KeyValueModel>> GetCompanyList()
        {
            return await _companyService.GetCompanyListAsync();
        }

        public async Task<CompanyModel> GetCompanyById(int companyId)
        {
            if (companyId <= 0)
                throw new Exception("Invalid Company Id");

            return await _companyService.GetCompanyByIdAsync(companyId);
        }
    }
}
