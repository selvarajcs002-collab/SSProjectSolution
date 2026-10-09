using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public interface IOutwardRepository
    {
        // ── Size-Based (existing — do NOT modify) ──────────────────────────────
        Task<IEnumerable<dynamic>> GetOutwardDetailsRawAsync(int id, string mode);
        Task<SSProjectSolution.Response.OutwardResponse> SaveOutwardAsync(Dapper.DynamicParameters parameters);
        Task<string> GenerateOutwardDcNoAsync(string? createdBy = null, int? companyId = null);
        Task<System.Collections.Generic.IEnumerable<ReusableDcNoDto>> GetReusableDcNosAsync(string? search, int? companyId);
        Task<ReuseDcNoResponse> ReserveReusedDcNoAsync(ReuseDcNoRequest request);
        Task ConfirmOutwardDcAllocationAsync(string dcNo, int outwardId, int? companyId, string? actor);
        Task<IEnumerable<dynamic>> GetAvailableSizesAsync(int companyId, string styleNo, string designName, string colour);

        // ── Meter-Based (new — isolated) ───────────────────────────────────────
        Task<OutwardMeterResponse> SaveMeterOutwardAsync(Dapper.DynamicParameters parameters);
        
        // ── Additional Details ─────────────────────────────────────────────────
        Task<dynamic> GetAdditionalDetailsOptionsAsync(int companyId);

        // ── Lot Completion ─────────────────────────────────────────────────────
        Task<dynamic> MarkLotCompletedAsync(Dapper.DynamicParameters parameters);
        Task<dynamic> MarkInwardInactiveAsync(Dapper.DynamicParameters parameters);
        Task<dynamic> MarkInwardInactiveByDcNoAsync(Dapper.DynamicParameters parameters);
    }
}
