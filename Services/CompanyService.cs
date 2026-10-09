using Dapper;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CompanyService> _logger;

        public CompanyService(DapperDBConnection dbConnection, ILogger<CompanyService> logger)
        {
            _dbConnection = dbConnection;
            _logger = logger;
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

            _logger.LogInformation(
                "Company {Mode}. CompanyId={CompanyId} Gst={MaskedGst}",
                request.Mode,
                request.CompanyId,
                CompanyGstResolver.Mask(request.Gst_No));

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

        public async Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string? query, int limit = 25)
        {
            using var connection = _dbConnection.CreateConnection();
            var term = (query ?? string.Empty).Trim();
            if (limit <= 0 || limit > 100)
                limit = 25;

            const string sql = @"
                SELECT TOP (@Limit)
                    companyId AS CompanyId,
                    companyName AS CompanyName,
                    ISNULL(gst_no, '') AS GstNo,
                    ISNULL(city, '') AS City
                FROM dbo.CompanyDetails
                WHERE (@Term = '' OR companyName LIKE '%' + @Term + '%' OR ISNULL(gst_no, '') LIKE '%' + @Term + '%' OR ISNULL(city, '') LIKE '%' + @Term + '%')
                ORDER BY companyName;";

            return await connection.QueryAsync<CompanySearchResult>(sql, new { Term = term, Limit = limit });
        }

        public async Task<CompanyModel> GetCompanyByIdAsync(int companyId)
        {
            using var connection = _dbConnection.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@companyId", companyId);

            var result = await connection.QueryFirstOrDefaultAsync(
                SPConstants.GetCompanyById,
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                _logger.LogWarning("Company lookup returned no row. CompanyId={CompanyId}", companyId);
                return new CompanyModel();
            }

            var row = (IDictionary<string, object>)result;
            var loadedCompanyId = ReadInt(row, "companyId");
            var loadedGst = ReadString(row, "gst_no");
            var gst = CompanyGstResolver.Resolve(companyId, loadedCompanyId, loadedGst);

            if (loadedCompanyId != companyId)
            {
                _logger.LogError(
                    "Company lookup returned a different company. RequestedCompanyId={RequestedCompanyId} ReturnedCompanyId={ReturnedCompanyId}",
                    companyId,
                    loadedCompanyId);
                return new CompanyModel();
            }

            var deliveryTo = ReadString(row, "deliveryToLocations");
            var model = new CompanyModel
            {
                CompanyId = loadedCompanyId,
                CompanyName = ReadString(row, "companyName"),
                Gst_No = gst,
                PhoneNumber = ReadString(row, "phoneNumber"),
                Door_No = ReadString(row, "door_no"),
                Street_Name = ReadString(row, "street_Name"),
                Landmark = ReadString(row, "landmark"),
                City = ReadString(row, "city"),
                Pincode = ReadString(row, "pincode"),
                DeliveryToLocations = string.IsNullOrWhiteSpace(deliveryTo)
                    ? new List<string>()
                    : Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(deliveryTo) ?? new List<string>()
            };

            _logger.LogInformation(
                "Company loaded. CompanyId={CompanyId} CompanyName={CompanyName} Gst={MaskedGst}",
                model.CompanyId,
                model.CompanyName,
                CompanyGstResolver.Mask(model.Gst_No));

            return model;
        }

        private static string ReadString(IDictionary<string, object> row, string name)
        {
            foreach (var entry in row)
            {
                if (!string.Equals(entry.Key, name, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (entry.Value == null || entry.Value is DBNull)
                    return string.Empty;

                return Convert.ToString(entry.Value)?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        private static int ReadInt(IDictionary<string, object> row, string name)
        {
            var value = ReadString(row, name);
            return int.TryParse(value, out var parsed) ? parsed : 0;
        }
    }
}
