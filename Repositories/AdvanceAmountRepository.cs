using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Repositories
{
    public class AdvanceAmountRepository : IAdvanceAmountRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public AdvanceAmountRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<AdvanceAmountDto>> GetAllAdvancesAsync()
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryAsync<AdvanceAmountDto>(
                "sp_GetAdvanceAmounts",
                commandType: CommandType.StoredProcedure);
        }

        public async Task<AdvanceAmountDto> GetAdvanceByIdAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<AdvanceAmountDto>(
                "sp_GetAdvanceAmountById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<int> AddAdvanceAsync(AddAdvanceAmountDto dto)
        {
            using var connection = _dbConnection.CreateConnection();
            var id = await connection.QuerySingleAsync<int>(
                "sp_InsertAdvanceAmount",
                new 
                { 
                    dto.Name, 
                    dto.Date, 
                    dto.Amount, 
                    dto.Remarks, 
                    dto.CreatedBy 
                },
                commandType: CommandType.StoredProcedure);
            return id;
        }

        public async Task<bool> UpdateAdvanceAsync(UpdateAdvanceAmountDto dto)
        {
            using var connection = _dbConnection.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_UpdateAdvanceAmount",
                new 
                { 
                    dto.Id, 
                    dto.Name, 
                    dto.Date, 
                    dto.Amount, 
                    dto.Remarks 
                },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAdvanceAsync(int id)
        {
            using var connection = _dbConnection.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_DeleteAdvanceAmount",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<AdvanceAmountDto>> GetAdvancesForExportAsync(DateTime fromDate, DateTime toDate)
        {
            using var connection = _dbConnection.CreateConnection();
            return await connection.QueryAsync<AdvanceAmountDto>(
                "sp_GetAdvanceAmountsForExport",
                new { FromDate = fromDate, ToDate = toDate },
                commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAdvanceChallanAsync(int id, string challanNo, string challanFileName)
        {
            using var connection = _dbConnection.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(
                "sp_UpdateAdvanceAmountChallan",
                new { Id = id, ChallanNo = challanNo, ChallanFileName = challanFileName },
                commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public async Task<string> AllocateNextChallanNoAsync(int advanceId, DateTime advanceDate)
        {
            using var connection = _dbConnection.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);

            try
            {
                var existing = await connection.QueryFirstOrDefaultAsync<string>(
                    "SELECT ChallanNo FROM dbo.AdvanceAmount WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id",
                    new { Id = advanceId },
                    transaction);

                if (!string.IsNullOrWhiteSpace(existing))
                {
                    transaction.Commit();
                    return existing;
                }

                var year = advanceDate.Year >= 2000 ? advanceDate.Year : DateTime.Now.Year;
                var prefix = $"AC-{year}-";

                var max = await connection.ExecuteScalarAsync<int?>(
                    @"SELECT MAX(TRY_CONVERT(INT, SUBSTRING(ChallanNo, LEN(@Prefix) + 1, 10)))
                      FROM dbo.AdvanceAmount WITH (UPDLOCK, HOLDLOCK)
                      WHERE ChallanNo LIKE @Prefix + '%'
                        AND TRY_CONVERT(INT, SUBSTRING(ChallanNo, LEN(@Prefix) + 1, 10)) IS NOT NULL",
                    new { Prefix = prefix },
                    transaction);

                var challanNo = prefix + ((max ?? 0) + 1).ToString("D3");

                var updated = await connection.ExecuteAsync(
                    @"UPDATE dbo.AdvanceAmount
                      SET ChallanNo = @ChallanNo
                      WHERE Id = @Id AND (ChallanNo IS NULL OR LTRIM(RTRIM(ChallanNo)) = '')",
                    new { Id = advanceId, ChallanNo = challanNo },
                    transaction);

                if (updated == 0)
                {
                    existing = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT ChallanNo FROM dbo.AdvanceAmount WITH (UPDLOCK, HOLDLOCK) WHERE Id = @Id",
                        new { Id = advanceId },
                        transaction);

                    if (!string.IsNullOrWhiteSpace(existing))
                        challanNo = existing;
                }

                transaction.Commit();
                return challanNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
