using SSProjectSolution.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public interface IDcDetailService
    {
        Task<IEnumerable<DcNoResponseDto>> GetDcNumbersByCompanyAsync(int companyId, string styleNo = null, string designName = null);

        /// <summary>
        /// Returns inward details for one or more DC numbers.
        /// A single DC produces the same response shape as the original API.
        /// </summary>
        Task<InwardEntryDetailsDto> GetInwardDetailsByDcsAsync(int companyId, IReadOnlyList<string> inwardDcNos, string colour = null);
    }
}
