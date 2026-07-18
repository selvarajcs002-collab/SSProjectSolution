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

        private DataTable CreateDcNumberTable(IEnumerable<string>? dcNumbers)
        {
            var table = new DataTable();
            table.Columns.Add("DcNo", typeof(string));
            if (dcNumbers != null)
            {
                foreach (var dc in dcNumbers)
                {
                    if (!string.IsNullOrWhiteSpace(dc))
                    {
                        table.Rows.Add(dc.Trim());
                    }
                }
            }
            return table;
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

            string spName = "usp_GetStockSummary";

            if (filter.DeliveryChallanBased)
            {
                spName = "usp_GetStockSummary_DcBased";
                var dcTable = CreateDcNumberTable(filter.DeliveryChallanNumbers);
                parameters.Add("@DcList", dcTable.AsTableValuedParameter("dbo.DcNumberList"));
            }

            return await connection.QueryFirstOrDefaultAsync<StockSummaryDto>(
                spName,
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

            string spName = "usp_GetStockBalance_SizeWise";

            if (filter.DeliveryChallanBased)
            {
                spName = "usp_GetStockBalance_SizeWise_DcBased";
                var dcTable = CreateDcNumberTable(filter.DeliveryChallanNumbers);
                parameters.Add("@DcList", dcTable.AsTableValuedParameter("dbo.DcNumberList"));
            }

            return await connection.QueryAsync<StockBalanceDto>(
                spName,
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

            string spName = "usp_GetLastTransactions";

            if (filter.DeliveryChallanBased)
            {
                spName = "usp_GetLastTransactions_DcBased";
                var dcTable = CreateDcNumberTable(filter.DeliveryChallanNumbers);
                parameters.Add("@DcList", dcTable.AsTableValuedParameter("dbo.DcNumberList"));
            }

            return await connection.QueryAsync<StockTransactionDto>(
                spName,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<DeliveryChallanDto>> GetDeliveryChallansAsync(int? companyId, string? styleNo, string? designName, string? colour, System.Threading.CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@StyleNo", styleNo);
            parameters.Add("@DesignName", designName);
            parameters.Add("@Colour", colour);

            return await connection.QueryAsync<DeliveryChallanDto>(
                new CommandDefinition(
                    "usp_GetDeliveryChallans_ForStock",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<FilterOptionDto>> GetStylesAsync(int companyId, System.Threading.CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);

            return await connection.QueryAsync<FilterOptionDto>(
                new CommandDefinition(
                    "usp_GetStyles_ForStock",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<FilterOptionDto>> GetDesignsAsync(int companyId, string styleNo, System.Threading.CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@StyleNo", styleNo);

            return await connection.QueryAsync<FilterOptionDto>(
                new CommandDefinition(
                    "usp_GetDesigns_ForStock",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }

        public async Task<IEnumerable<FilterOptionDto>> GetColoursAsync(int companyId, string styleNo, string designName, System.Threading.CancellationToken cancellationToken = default)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@StyleNo", styleNo);
            parameters.Add("@DesignName", designName);

            return await connection.QueryAsync<FilterOptionDto>(
                new CommandDefinition(
                    "usp_GetColours_ForStock",
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    cancellationToken: cancellationToken));
        }
    }
}
