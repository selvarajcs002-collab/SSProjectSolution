using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IOutwardService
    {
        // ── Size-Based (existing — do NOT modify) ──────────────────────────────
        Task<OutwardResponse> SaveOutwardAsync(OutwardRequest request);
        Task<OutwardByDcResponseDto?> GetOutwardByDcNoAsync(int id, string mode);
        Task<OutwardResponse> UpdateOutwardAsync(OutwardUpdateRequest request);
        Task<System.Collections.Generic.IEnumerable<dynamic>> GetAvailableSizesAsync(int companyId, string styleNo, string designName, string colour);

        // ── Meter-Based (new — isolated) ───────────────────────────────────────
        Task<OutwardMeterResponse> SaveMeterOutwardAsync(OutwardMeterSaveRequest request);
    }
}
