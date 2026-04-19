using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public class DcFilterService : IDcFilterService
    {
        private readonly DapperDBConnection _dbConnection;

        public DcFilterService(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<InwardOutwardNestedResponse> GetInwardOutwardDetailsAsync(InwardOutwardFilterRequest request)
        {
            var response = new InwardOutwardNestedResponse();
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Mode", request.Mode);
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@StyleNo", request.StyleNo);
                parameters.Add("@DesignName", request.DesignName);

                using var multi = await connection.QueryMultipleAsync(
                    SPConstants.GetInwardOutwardDetailsFilter,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // Determine what to read based on Mode
                bool readInward = string.IsNullOrEmpty(request.Mode) || request.Mode.ToUpper() == "INWARD";
                bool readOutward = string.IsNullOrEmpty(request.Mode) || request.Mode.ToUpper() == "OUTWARD";

                if (readInward)
                {
                    var inwardRaw = (await multi.ReadAsync<dynamic>()).ToList();
                    response.Inward = MapGroupedItems(inwardRaw, true);
                }

                if (readOutward)
                {
                    var outwardRaw = (await multi.ReadAsync<dynamic>()).ToList();
                    response.Outward = MapGroupedItems(outwardRaw, false);
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInwardOutwardDetailsAsync: {ex.Message}");
                return response;
            }
        }

        public async Task<InwardOutwardNestedResponse> GetInwardOutwardAsync(string mode, int pageNumber = 1, int pageSize = 10)
        {
            var response = new InwardOutwardNestedResponse();
            try
            {
                using var connection = _dbConnection.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Mode", mode);
                parameters.Add("@PageNumber", pageNumber);
                parameters.Add("@PageSize", pageSize);

                using var multi = await connection.QueryMultipleAsync(
                    SPConstants.GetInwardOutwardDetails,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                bool readInward = string.IsNullOrEmpty(mode) || mode.ToUpper() == "INWARD";
                bool readOutward = string.IsNullOrEmpty(mode) || mode.ToUpper() == "OUTWARD";

                if (readInward)
                {
                    var inwardRaw = (await multi.ReadAsync<dynamic>()).ToList();
                    response.Inward = MapGroupedItems(inwardRaw, true);
                }

                if (readOutward)
                {
                    var outwardRaw = (await multi.ReadAsync<dynamic>()).ToList();
                    response.Outward = MapGroupedItems(outwardRaw, false);
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInwardOutwardAsync: {ex.Message}");
                return response;
            }
        }

        private List<InwardOutwardItem> MapGroupedItems(List<dynamic> rawData, bool isInward)
        {
            if (rawData == null || !rawData.Any()) return new List<InwardOutwardItem>();

            return rawData
                .Where(x => (isInward ? x.InwardId : x.OutwardId) != null)
                .GroupBy(x => (int)(isInward ? x.InwardId : x.OutwardId))
                .Select(g => new InwardOutwardItem
                {
                    Id = g.Key,
                    CompanyName = g.First().CompanyName,
                    CompanyId = g.First().CompanyId,
                    Colour = g.First().Colour,
                    DesignName = g.First().DesignName,
                    StyleNo = g.First().StyleNo,
                    UploadURL = g.First().UploadURL,
                    CreatedBy = g.First().CreatedBy?.ToString(),
                    CreatedDate = g.First().CreatedDate,
                    UpdatedDate = g.First().UpdatedDate,
                    DcNo = isInward ? g.First().InwardDcNo : g.First().OutwardDcNo,
                    Status = isInward ? g.First().Status : g.First().Status, // Status is always 'Status' in your SP
                    SizeCounts = g.Where(s => s.SizeCountId != null).Select(s => new SizeCountDto
                    {
                        SizeCountId = s.SizeCountId,
                        Size = s.Size,
                        Count = s.Count
                    }).ToList()
                }).ToList();
        }
    }
}
