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

        // ── Meter-Based (new — isolated) ───────────────────────────────────────
        Task<OutwardMeterResponse> SaveMeterOutwardAsync(OutwardMeterSaveRequest request);
    }
}
