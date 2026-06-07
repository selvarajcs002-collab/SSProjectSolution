using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using System.Data;

namespace SSProjectSolution.Repositories
{
    public class InwardRepository : IInwardRepository
    {
        private readonly DapperDBConnection _dbConnection;

        public InwardRepository(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<SizeResponseDto>> GetSizesByColourStyleAsync(int companyId, string colour, string styleNo)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Colour", colour);
            parameters.Add("@StyleNo", styleNo);

            return await connection.QueryAsync<SizeResponseDto>(
                SPConstants.GetSizesByColourStyle,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<MeterResponseDto>> GetMetersByColourStyleAsync(int companyId, string colour, string styleNo)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@Colour", colour);
            parameters.Add("@StyleNo", styleNo);

            return await connection.QueryAsync<MeterResponseDto>(
                SPConstants.GetMetersByColourStyle,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<InwardByDcResponseDto> GetInwardByCompanyAndDcAsync(int companyId, string inwardDcNo)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);
            parameters.Add("@InwardDcNo", inwardDcNo);

            return await connection.QueryFirstOrDefaultAsync<InwardByDcResponseDto>(
                SPConstants.GetInwardByCompanyAndDc,
                parameters,
                commandType: CommandType.StoredProcedure);
        }

        public async Task<string> UpdateInwardAsync(InwardUpdateDto request)
        {
            using var connection = _dbConnection.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@InwardId", request.InwardId);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@Colour", request.Colour);
                parameters.Add("@DesignName", request.DesignName);
                parameters.Add("@StyleNo", request.StyleNo);
                parameters.Add("@InwardDcNo", request.InwardDcNo);
                parameters.Add("@UpdatedBy", request.UpdatedBy);

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    SPConstants.UpdateInward,
                    parameters,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                if (request.EntryType == 'M')
                {
                    await connection.ExecuteAsync(
                        "DELETE FROM INWARD_METER_DETAIL WHERE IMD_INWARD_ID = @InwardId",
                        new { InwardId = request.InwardId },
                        transaction);

                    if (request.MeterDetails != null && request.MeterDetails.Any())
                    {
                        var validMeters = request.MeterDetails.Where(m => m.MeterValue > 0 && m.BitsCount > 0).ToList();
                        if (validMeters.Any())
                        {
                            var meterSql = @"
                                INSERT INTO INWARD_METER_DETAIL (IMD_INWARD_ID, IMD_COMPANY_ID, IMD_METER_VALUE, IMD_BITS_COUNT, IMD_TOTAL_METER, IMD_CREATED_BY, IMD_CREATED_DATE)
                                VALUES (@InwardId, @CompanyId, @MeterValue, @BitsCount, (@MeterValue * @BitsCount), @UpdatedBy, GETDATE())";
                            
                            var meterParams = validMeters.Select(m => new {
                                InwardId = request.InwardId,
                                CompanyId = request.CompanyId,
                                MeterValue = m.MeterValue,
                                BitsCount = m.BitsCount,
                                UpdatedBy = request.UpdatedBy
                            }).ToList();

                            await connection.ExecuteAsync(meterSql, meterParams, transaction);
                        }
                    }
                }
                else
                {
                    await connection.ExecuteAsync(
                        "DELETE FROM InwardSizeCount WHERE InwardId = @InwardId",
                        new { InwardId = request.InwardId },
                        transaction);

                    if (request.Sizes != null && request.Sizes.Any())
                    {
                        var validSizes = request.Sizes.Where(s => s.Count > 0 && !string.IsNullOrWhiteSpace(s.Size)).ToList();
                        if (validSizes.Any())
                        {
                            var sizeSql = @"
                                INSERT INTO InwardSizeCount (InwardId, StyleNo, DesignName, Colour, Size, Count)
                                VALUES (@InwardId, @StyleNo, @DesignName, @Colour, LTRIM(RTRIM(@Size)), @Count)";

                            var sizeParams = validSizes.Select(s => new {
                                InwardId = request.InwardId,
                                StyleNo = request.StyleNo,
                                DesignName = request.DesignName,
                                Colour = request.Colour,
                                Size = s.Size,
                                Count = s.Count
                            }).ToList();

                            await connection.ExecuteAsync(sizeSql, sizeParams, transaction);
                        }
                    }
                }

                transaction.Commit();
                return result?.message ?? "Inward updated successfully";
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<DesignStyleColourDto>> GetDesignStyleColourByCompanyAsync(int companyId)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", companyId);

            return await connection.QueryAsync<DesignStyleColourDto>(
                SPConstants.GetDesignStyleColourByCompany,
                parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
