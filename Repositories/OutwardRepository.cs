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

        public async Task<IEnumerable<dynamic>> GetAvailableSizesAsync(int companyId, string styleNo, string designName, string colour)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = @"
                WITH InwardStock AS (
                    SELECT Size, SUM([Count]) as TotalInward
                    FROM InwardSizeCount
                    WHERE StyleNo = @StyleNo AND DesignName = @DesignName AND Colour = @Colour
                    GROUP BY Size
                ),
                OutwardUsed AS (
                    SELECT Size, SUM([Count]) as TotalOutward
                    FROM OutwardSizeCount
                    WHERE StyleNo = @StyleNo AND DesignName = @DesignName AND Colour = @Colour
                    GROUP BY Size
                )
                SELECT 
                    0 as sizeId,
                    i.Size as sizeName,
                    (ISNULL(i.TotalInward, 0) - ISNULL(o.TotalOutward, 0)) as availableQty
                FROM InwardStock i
                LEFT JOIN OutwardUsed o ON i.Size = o.Size
                WHERE (ISNULL(i.TotalInward, 0) - ISNULL(o.TotalOutward, 0)) > 0;
            ";

            var parameters = new { CompanyId = companyId, StyleNo = styleNo, DesignName = designName, Colour = colour };
            return await connection.QueryAsync<dynamic>(sql, parameters);
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

        // ── Additional Details ─────────────────────────────────────────────────

        public async Task<dynamic> GetAdditionalDetailsOptionsAsync(int companyId)
        {
            using var connection = _dbConnection.CreateConnection();
            var sql = @"
                SELECT DISTINCT DeliveryTo FROM Outward WHERE CompanyId = @CompanyId AND DeliveryTo IS NOT NULL AND DeliveryTo <> '';
                SELECT DISTINCT PoNo FROM Outward WHERE CompanyId = @CompanyId AND PoNo IS NOT NULL AND PoNo <> '';
            ";
            
            using var multi = await connection.QueryMultipleAsync(sql, new { CompanyId = companyId });
            
            var deliveryLocations = await multi.ReadAsync<string>();
            var poNumbers = await multi.ReadAsync<string>();
            
            return new { DeliveryLocations = deliveryLocations, PoNumbers = poNumbers };
        }
    }
}
