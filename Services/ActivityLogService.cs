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
    public class ActivityLogService : IActivityLogService
    {
        private readonly DapperDBConnection _dbConnection;

        public ActivityLogService(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<ActivityLogResponse> GetActivityLogAsync(ActivityLogRequest request)
        {
            var response = new ActivityLogResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                using var connection = _dbConnection.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@Module", request.Module ?? "INWARD");
                parameters.Add("@ViewType", request.ViewType ?? "S");
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);
                parameters.Add("@CompanyId", request.CompanyId);
                parameters.Add("@StyleNo", request.StyleNo);
                parameters.Add("@DesignName", request.DesignName);
                parameters.Add("@PageNumber", request.PageNumber);
                parameters.Add("@PageSize", request.PageSize);
                parameters.Add("@SortColumn", request.SortColumn);
                parameters.Add("@SortDirection", request.SortDirection);

                using var multi = await connection.QueryMultipleAsync(
                    "SP_GET_ACTIVITY_LOG",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                // 1. Read Summary
                var summaryRow = await multi.ReadFirstOrDefaultAsync<dynamic>();
                if (summaryRow != null)
                {
                    response.TotalRecords = (int)(summaryRow.TotalRecords ?? 0);
                    response.Summary.TotalBitsCount = (decimal)(summaryRow.TotalBitsCount ?? 0);
                    response.Summary.TotalMeter = (decimal)(summaryRow.TotalMeter ?? 0);
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

                response.Data = masterData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetActivityLogAsync: {ex.Message}");
            }

            return response;
        }

        public async Task<ActivityLogResponse> AdvancedFilterAsync(ActivityLogRequest request)
        {
            var response = new ActivityLogResponse
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                
                string module = request.Module?.ToUpper() ?? "INWARD";
                string viewType = request.ViewType?.ToUpper() ?? "S";
                
                var whereBuilder = new System.Text.StringBuilder();
                
                string tableName = module == "INWARD" ? "Inward i" : "Outward o";
                string alias = module == "INWARD" ? "i" : "o";
                string idColumn = module == "INWARD" ? "InwardId" : "OutwardId";
                string dcColumn = module == "INWARD" ? "InwardDcNo" : "OutwardDcNo";
                string entryTypeColumn = module == "INWARD" ? "InwardEntryType" : "OutwardEntryType";
                
                string bitsQuery = "0 AS TotalBitsCount";
                string meterQuery = "0 AS TotalMeter";

                if (module == "INWARD")
                {
                    if (viewType == "S")
                    {
                        bitsQuery = "(SELECT ISNULL(SUM(isc.Count), 0) FROM InwardSizeCount isc WHERE isc.InwardId = i.InwardId) AS TotalBitsCount";
                        whereBuilder.Append($" WHERE ({entryTypeColumn} = 'S' OR {entryTypeColumn} IS NULL)");
                    }
                    else
                    {
                        meterQuery = "(SELECT ISNULL(SUM(imd.IMD_TOTAL_METER), 0) FROM INWARD_METER_DETAIL imd WHERE imd.IMD_INWARD_ID = i.InwardId) AS TotalMeter";
                        whereBuilder.Append($" WHERE {entryTypeColumn} = 'M'");
                    }
                }
                else
                {
                    if (viewType == "S")
                    {
                        bitsQuery = "(SELECT ISNULL(SUM(osc.Count), 0) FROM OutwardSizeCount osc WHERE osc.OutwardId = o.OutwardId) AS TotalBitsCount";
                        whereBuilder.Append($" WHERE ({entryTypeColumn} = 'S' OR {entryTypeColumn} IS NULL)");
                    }
                    else
                    {
                        meterQuery = "(SELECT ISNULL(SUM(omd.OMD_TOTAL_METER), 0) FROM OUTWARD_METER_DETAIL omd WHERE omd.OMD_OUTWARD_ID = o.OutwardId) AS TotalMeter";
                        whereBuilder.Append($" WHERE {entryTypeColumn} = 'M'");
                    }
                }
                
                if (request.FromDate.HasValue)
                {
                    whereBuilder.Append($" AND CAST({alias}.CreatedDate AS DATE) >= @FromDate");
                    parameters.Add("@FromDate", request.FromDate.Value.Date);
                }
                if (request.ToDate.HasValue)
                {
                    whereBuilder.Append($" AND CAST({alias}.CreatedDate AS DATE) <= @ToDate");
                    parameters.Add("@ToDate", request.ToDate.Value.Date);
                }
                if (request.CompanyId.HasValue && request.CompanyId.Value > 0)
                {
                    whereBuilder.Append($" AND {alias}.CompanyId = @CompanyId");
                    parameters.Add("@CompanyId", request.CompanyId.Value);
                }
                if (!string.IsNullOrEmpty(request.StyleNo))
                {
                    whereBuilder.Append($" AND {alias}.StyleNo = @StyleNo");
                    parameters.Add("@StyleNo", request.StyleNo);
                }
                if (!string.IsNullOrEmpty(request.DesignName))
                {
                    whereBuilder.Append($" AND {alias}.DesignName = @DesignName");
                    parameters.Add("@DesignName", request.DesignName);
                }

                // Base query
                string baseQuery = $@"
                    SELECT 
                        {alias}.{idColumn} AS Id,
                        {alias}.CompanyId,
                        c.companyName AS CompanyName,
                        {alias}.{dcColumn} AS DcNo,
                        {alias}.CreatedDate AS Date,
                        {alias}.StyleNo,
                        {alias}.DesignName,
                        {alias}.Colour,
                        {bitsQuery},
                        {meterQuery}
                    FROM {tableName}
                    LEFT JOIN CompanyDetails c ON {alias}.CompanyId = c.companyId
                    {whereBuilder.ToString()}
                ";

                // Summary Query
                string summaryQuery = $@"
                    WITH FilteredData AS ({baseQuery})
                    SELECT 
                        COUNT(*) AS TotalRecords,
                        ISNULL(SUM(TotalBitsCount), 0) AS TotalBitsCount,
                        ISNULL(SUM(TotalMeter), 0) AS TotalMeter
                    FROM FilteredData;
                ";

                // Sort & Pagination
                string sortColumn = request.SortColumn ?? "Date";
                string sortDir = request.SortDirection?.ToUpper() == "ASC" ? "ASC" : "DESC";
                
                string orderBy = sortColumn switch
                {
                    "DcNo" => "DcNo",
                    "StyleNo" => "StyleNo",
                    _ => "Date"
                };

                int offset = (request.PageNumber - 1) * request.PageSize;

                string paginatedQuery = $@"
                    WITH FilteredData AS ({baseQuery})
                    SELECT * FROM FilteredData
                    ORDER BY {orderBy} {sortDir}, Id DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
                ";
                parameters.Add("@Offset", offset);
                parameters.Add("@PageSize", request.PageSize);

                // Details Query
                string detailsQuery = "";
                if (module == "INWARD" && viewType == "S")
                    detailsQuery = $@"
                        WITH FilteredData AS ({baseQuery}),
                             Paginated AS (SELECT Id FROM FilteredData ORDER BY {orderBy} {sortDir}, Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                        SELECT isc.Id, isc.InwardId AS ParentId, isc.Size, isc.Count 
                        FROM InwardSizeCount isc
                        INNER JOIN Paginated p ON isc.InwardId = p.Id;
                    ";
                else if (module == "INWARD" && viewType == "M")
                    detailsQuery = $@"
                        WITH FilteredData AS ({baseQuery}),
                             Paginated AS (SELECT Id FROM FilteredData ORDER BY {orderBy} {sortDir}, Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                        SELECT imd.IMD_ID AS Id, imd.IMD_INWARD_ID AS ParentId, imd.IMD_METER_VALUE AS MeterValue, imd.IMD_BITS_COUNT AS BitsCount, imd.IMD_TOTAL_METER AS TotalMeter
                        FROM INWARD_METER_DETAIL imd
                        INNER JOIN Paginated p ON imd.IMD_INWARD_ID = p.Id;
                    ";
                else if (module == "OUTWARD" && viewType == "S")
                    detailsQuery = $@"
                        WITH FilteredData AS ({baseQuery}),
                             Paginated AS (SELECT Id FROM FilteredData ORDER BY {orderBy} {sortDir}, Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                        SELECT osc.Id, osc.OutwardId AS ParentId, osc.Size, osc.Count 
                        FROM OutwardSizeCount osc
                        INNER JOIN Paginated p ON osc.OutwardId = p.Id;
                    ";
                else if (module == "OUTWARD" && viewType == "M")
                    detailsQuery = $@"
                        WITH FilteredData AS ({baseQuery}),
                             Paginated AS (SELECT Id FROM FilteredData ORDER BY {orderBy} {sortDir}, Id DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY)
                        SELECT omd.OMD_ID AS Id, omd.OMD_OUTWARD_ID AS ParentId, omd.OMD_METER_VALUE AS MeterValue, omd.OMD_BITS_COUNT AS BitsCount, omd.OMD_TOTAL_METER AS TotalMeter
                        FROM OUTWARD_METER_DETAIL omd
                        INNER JOIN Paginated p ON omd.OMD_OUTWARD_ID = p.Id;
                    ";

                string finalSql = summaryQuery + paginatedQuery + detailsQuery;

                using var multi = await connection.QueryMultipleAsync(finalSql, parameters, commandType: CommandType.Text);

                var summaryRow = await multi.ReadFirstOrDefaultAsync<dynamic>();
                if (summaryRow != null)
                {
                    response.TotalRecords = (int)(summaryRow.TotalRecords ?? 0);
                    response.Summary.TotalBitsCount = (decimal)(summaryRow.TotalBitsCount ?? 0);
                    response.Summary.TotalMeter = (decimal)(summaryRow.TotalMeter ?? 0);
                }

                var masterData = (await multi.ReadAsync<ActivityLogItem>()).ToList();
                var detailData = (await multi.ReadAsync<ActivityLogDetail>()).ToList();

                foreach (var item in masterData)
                {
                    item.Details = detailData.Where(d => d.ParentId == item.Id).ToList();
                }

                response.Data = masterData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AdvancedFilterAsync: {ex.Message}");
            }

            return response;
        }
    }
}
