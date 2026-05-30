using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Response;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SSProjectSolution.Repositories
{
    public class OutwardRepository : IOutwardRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public OutwardRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        // ── Size-Based (existing — untouched) ──────────────────────────────────

        public async Task<IEnumerable<dynamic>> GetOutwardDetailsRawAsync(int id, string mode)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            parameters.Add("@Mode", mode);

            return await connection.QueryAsync<dynamic>(
                SPConstants.GetDetailsByIdMode,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<SSProjectSolution.Response.OutwardResponse> SaveOutwardAsync(DynamicParameters parameters)
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryFirstAsync<SSProjectSolution.Response.OutwardResponse>(
                SPConstants.SaveOutward,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        // ── Meter-Based (new — isolated) ───────────────────────────────────────

        public async Task<OutwardMeterResponse> SaveMeterOutwardAsync(DynamicParameters parameters)
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryFirstAsync<OutwardMeterResponse>(
                SPConstants.SaveMeterOutward,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
