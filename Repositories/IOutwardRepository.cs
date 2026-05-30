using System.Collections.Generic;
using System.Threading.Tasks;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public interface IOutwardRepository
    {
        // ── Size-Based (existing — do NOT modify) ──────────────────────────────
        Task<IEnumerable<dynamic>> GetOutwardDetailsRawAsync(int id, string mode);
        Task<SSProjectSolution.Response.OutwardResponse> SaveOutwardAsync(Dapper.DynamicParameters parameters);

        // ── Meter-Based (new — isolated) ───────────────────────────────────────
        Task<OutwardMeterResponse> SaveMeterOutwardAsync(Dapper.DynamicParameters parameters);
    }
}
