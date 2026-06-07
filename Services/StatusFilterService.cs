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
    public class StatusFilterService : IStatusFilterService
    {
        private readonly DapperDBConnection _dbConnection;

        public StatusFilterService(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<StatusFilterResponse> SearchAsync(StatusFilterRequest request)
        {
            var response = new StatusFilterResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@StyleId", string.IsNullOrEmpty(request.StyleId) ? null : request.StyleId);
                parameters.Add("@DesignId", string.IsNullOrEmpty(request.DesignId) ? null : request.DesignId);
                parameters.Add("@TransactionType", request.TransactionType?.ToUpper() ?? "INWARD");
                parameters.Add("@ViewType", request.ViewType?.ToUpper() ?? "SIZE");
                parameters.Add("@PageNumber", request.PageNumber);
                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@SortColumn", request.SortColumn);
                parameters.Add("@SortDirection", request.SortDirection);

                using var multi = await connection.QueryMultipleAsync(
                    "SP_GET_STATUS_FILTER",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // 1. Read Summary
                var summaryRow = await multi.ReadFirstOrDefaultAsync<dynamic>();
                if (summaryRow != null)
                {
                    response.TotalRecords = (int)(summaryRow.TotalRecords ?? 0);
                    response.Summary = new 
                    {
                        TotalBitsCount = (decimal)(summaryRow.TotalBitsCount ?? 0),
                        TotalMeter = (decimal)(summaryRow.TotalMeter ?? 0)
                    };
                }

                // 2. Read Paginated Master Data
                var masterData = (await multi.ReadAsync<ActivityLogItem>()).ToList();

                // 3. Read Detailed Rows
                var detailData = (await multi.ReadAsync<ActivityLogDetail>()).ToList();

                // 4. Map Details to Master Data
                foreach (var item in masterData)
                {
                    item.Details = detailData.Where(d => d.ParentId == item.Id).ToList();
                }

                response.Data = masterData.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StatusFilterService: {ex.Message}");
                response.Success = false;
                response.Message = "An error occurred while filtering records.";
            }

            return response;
        }
    }
}
