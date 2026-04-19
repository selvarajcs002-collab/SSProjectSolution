using Dapper;
using SSProjectSolution.Data;
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
    }
}
