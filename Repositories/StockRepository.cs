using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public StockRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<StockSummaryDto> GetStockSummaryAsync(StockFilterRequest filter)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", filter.FromDate);
            parameters.Add("@ToDate", filter.ToDate);
            parameters.Add("@CompanyId", filter.CompanyId);
            parameters.Add("@StyleNo", filter.StyleNo);
            parameters.Add("@DesignName", filter.DesignName);
            parameters.Add("@Colour", filter.Colour);

            return await connection.QueryFirstOrDefaultAsync<StockSummaryDto>(
                "usp_GetStockSummary",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StockBalanceDto>> GetStockBalanceAsync(StockFilterRequest filter)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", filter.FromDate);
            parameters.Add("@ToDate", filter.ToDate);
            parameters.Add("@CompanyId", filter.CompanyId);
            parameters.Add("@StyleNo", filter.StyleNo);
            parameters.Add("@DesignName", filter.DesignName);
            parameters.Add("@Colour", filter.Colour);

            return await connection.QueryAsync<StockBalanceDto>(
                "usp_GetStockBalance_SizeWise",
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<StockTransactionDto>> GetLastTransactionsAsync(StockFilterRequest filter)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", filter.FromDate);
            parameters.Add("@ToDate", filter.ToDate);
            parameters.Add("@CompanyId", filter.CompanyId);
            parameters.Add("@StyleNo", filter.StyleNo);
            parameters.Add("@DesignName", filter.DesignName);
            parameters.Add("@Colour", filter.Colour);
            parameters.Add("@TopCount", 50);

            return await connection.QueryAsync<StockTransactionDto>(
                "usp_GetLastTransactions",
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
