using SSProjectSolution.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public interface IDcDetailRepository
    {
        Task<IEnumerable<DcNoResponseDto>> GetDcNumbersByCompanyAsync(int companyId, string styleNo = null, string designName = null);

        /// <summary>
        /// Fetches inward details for one or more DC numbers in a single DB round-trip.
        /// When a single DC is supplied the response shape is identical to the legacy endpoint.
        /// </summary>
        Task<InwardEntryDetailsDto> GetInwardDetailsByDcsAsync(int companyId, IReadOnlyList<string> inwardDcNos, string colour = null);
    }
}
