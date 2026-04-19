using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using System.Data;

namespace SSProjectSolution.Services
{
    public class UserService : IUserService
    {
        private readonly DapperDBConnection _dbConnection;

        public UserService(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CommonResponse> ManageUserAsync(UserRequest request)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@mode", request.Mode);
            parameters.Add("@userId", request.UserId);
            parameters.Add("@email", request.Email);
            parameters.Add("@password", request.Password);
            parameters.Add("@createdBy", request.CreatedBy);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.ManageUser, 
                parameters, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<CommonResponse> LoginUserAsync(LoginRequest request)
        {
            using var connection = _dbConnection.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@email", request.Email);
            parameters.Add("@password", request.Password);

            var result = await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.LoginUser,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result ?? new CommonResponse { Id = 0, Message = "Invalid Response", Status = false };
        }
    }
}
