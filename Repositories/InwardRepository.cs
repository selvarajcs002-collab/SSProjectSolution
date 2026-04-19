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
                commandType: CommandType.StoredProcedure);

            return result?.message ?? "Update failed";
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
