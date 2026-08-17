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
        Task<System.Collections.Generic.IEnumerable<dynamic>> GetColoursByDcsAsync(int companyId, string styleNo, string designName, System.Collections.Generic.List<string> dcNos);

        // ── Meter-Based (new — isolated) ───────────────────────────────────────
        Task<OutwardMeterResponse> SaveMeterOutwardAsync(OutwardMeterSaveRequest request);
        
        // ── Additional Details ─────────────────────────────────────────────────
        Task<dynamic> GetAdditionalDetailsOptionsAsync(int companyId);

        // ── Lot Completion ─────────────────────────────────────────────────────
        Task<dynamic> MarkLotCompletedAsync(LotCompletedDto payload);
        Task<dynamic> MarkInwardInactiveAsync(InwardStatusUpdateDto payload);
        Task<dynamic> MarkInwardInactiveByDcNoAsync(InwardStatusUpdateByDcNoDto payload);
    }
}
