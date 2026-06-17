using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class DcDetailService : IDcDetailService
    {
        private readonly IDcDetailRepository _repository;

        public DcDetailService(IDcDetailRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DcNoResponseDto>> GetDcNumbersByCompanyAsync(
            int companyId, string styleNo = null, string designName = null)
        {
            return await _repository.GetDcNumbersByCompanyAsync(companyId, styleNo, designName);
        }

        /// <summary>
        /// Passes through to the repository with the full list of DC numbers.
        /// The response shape is identical whether 1 or N DCs are requested.
        /// </summary>
        public async Task<InwardEntryDetailsDto> GetInwardDetailsByDcsAsync(
            int companyId, IReadOnlyList<string> inwardDcNos, string colour = null)
        {
            return await _repository.GetInwardDetailsByDcsAsync(companyId, inwardDcNos, colour);
        }
    }
}
