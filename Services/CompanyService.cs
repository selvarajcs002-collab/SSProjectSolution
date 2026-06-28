using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using System.Data;

namespace SSProjectSolution.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly DapperDBConnection _dbConnection;

        public CompanyService(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CommonResponse> ManageCompanyAsync(CompanyRequest request)
        {
            using var connection = _dbConnection.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@mode", request.Mode);
            parameters.Add("@companyId", request.CompanyId);
            parameters.Add("@companyName", request.CompanyName);
            parameters.Add("@gst_no", request.Gst_No);
            parameters.Add("@phoneNumber", request.PhoneNumber);
            parameters.Add("@door_no", request.Door_No);
            parameters.Add("@street_Name", request.Street_Name);
            parameters.Add("@landmark", request.Landmark);
            parameters.Add("@city", request.City);
            parameters.Add("@pincode", request.Pincode);
            parameters.Add("@deliveryToLocations", request.DeliveryToLocations != null ? Newtonsoft.Json.JsonConvert.SerializeObject(request.DeliveryToLocations) : null);

            return await connection.QueryFirstOrDefaultAsync<CommonResponse>(
                SPConstants.ManageCompany, 
                parameters, 
                commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<KeyValueModel>> GetCompanyListAsync()
        {
            using var connection = _dbConnection.CreateConnection();

            return await connection.QueryAsync<KeyValueModel>(
                SPConstants.GetCompanyList,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<CompanyModel> GetCompanyByIdAsync(int companyId)
        {
            using var connection = _dbConnection.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@companyId", companyId);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                SPConstants.GetCompanyById,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return new CompanyModel();

            return new CompanyModel
            {
                CompanyId = result.companyId ?? 0,
                CompanyName = result.companyName ?? string.Empty,
                Gst_No = result.gst_no ?? string.Empty,
                PhoneNumber = result.phoneNumber ?? string.Empty,
                Door_No = result.door_no ?? string.Empty,
                Street_Name = result.street_Name ?? string.Empty,
                Landmark = result.landmark ?? string.Empty,
                City = result.city ?? string.Empty,
                Pincode = result.pincode ?? string.Empty,
                DeliveryToLocations = string.IsNullOrEmpty((string)result.deliveryToLocations)
                    ? new List<string>()
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>((string)result.deliveryToLocations) ?? new List<string>()
            };
        }
    }
}
