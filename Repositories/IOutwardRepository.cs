using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public interface IOutwardRepository
    {
        Task<IEnumerable<dynamic>> GetOutwardDetailsRawAsync(int id, string mode);
        Task<SSProjectSolution.Response.OutwardResponse> SaveOutwardAsync(Dapper.DynamicParameters parameters);
    }
}
