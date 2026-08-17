using Dapper;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSProjectSolution.Services
{
    public class ExcelReportService : IExcelReportService
    {
        private readonly DapperDBConnection _dbConnection;
        private readonly IStockService _stockService;

        public ExcelReportService(DapperDBConnection dbConnection, IStockService stockService)
        {
            _dbConnection = dbConnection;
            _stockService = stockService;
        }

        public async Task<ReportResponseDto> GetDeliveryChallanReportAsync(ReportFilterRequestDto request)
        {
            using var connection = _dbConnection.CreateConnection();
            
            var parameters = new DynamicParameters();
            parameters.Add("@FromDate", request.FromDate);
            parameters.Add("@ToDate", request.ToDate);
            parameters.Add("@Mode", request.Mode);
            parameters.Add("@Type", request.Type);
            parameters.Add("@CompanyId", request.CompanyId);
            parameters.Add("@StyleNo", request.StyleNo);
            parameters.Add("@DesignName", request.DesignName);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetDeliveryChallanExcelReport",
                parameters,
                commandType: CommandType.StoredProcedure);

            var flatData = (await multi.ReadAsync<dynamic>()).ToList();
            var summaryData = await multi.ReadFirstOrDefaultAsync<dynamic>();

            var response = new ReportResponseDto
            {
                Summary = new ReportSummaryDto
                {
                    TotalRecords = summaryData != null ? (int)summaryData.TotalRecords : 0,
                    TotalBitsCount = summaryData != null ? (int)summaryData.TotalBitsCount : 0,
                    TotalMeter = summaryData != null ? (decimal)summaryData.TotalMeter : 0
                },
                DynamicColumns = new List<string>(),
                Data = new List<ReportDataRowDto>()
            };

            if (!flatData.Any())
                return response;

            var columns = flatData
                .Where(x => !string.IsNullOrEmpty((string)x.SizeName))
                .Select(x => (string)x.SizeName)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            response.DynamicColumns = columns;

            var groupedData = flatData.GroupBy(x => new {
                DCNo = (string)x.DCNo,
                Date = (string)x.Date,
                StyleNo = (string)x.StyleNo,
                DesignName = (string)x.DesignName,
                Colour = (string)x.Colour
            });

            int sno = 1;
            foreach (var group in groupedData)
            {
                var row = new ReportDataRowDto
                {
                    Sno = sno++,
                    DcNo = group.Key.DCNo,
                    Date = group.Key.Date,
                    StyleNo = group.Key.StyleNo,
                    DesignName = group.Key.DesignName,
                    Colour = group.Key.Colour,
                    TotalBits = group.Sum(x => (int)x.TotalBits),
                    TotalMeter = request.Type?.ToLower() == "meter" ? group.Sum(x => (decimal)x.Quantity) : 0,
                    DynamicValues = new Dictionary<string, decimal>()
                };

                foreach (var col in columns)
                {
                    row.DynamicValues[col] = 0;
                }

                foreach (var item in group)
                {
                    string colName = (string)item.SizeName;
                    if (!string.IsNullOrEmpty(colName))
                    {
                        row.DynamicValues[colName] += (decimal)item.TotalBits;
                    }
                }

                response.Data.Add(row);
            }

            return response;
        }
        public async Task<SSProjectSolution.Response.StockManagementReportDto> GetStockManagementReportAsync(SSProjectSolution.Request.StockFilterRequest request)
        {
            var summary = await _stockService.GetStockSummaryAsync(request);
            var balance = await _stockService.GetStockBalanceAsync(request);
            var transactions = await _stockService.GetLastTransactionsAsync(request);
            
            // To get CompanyName we could query it or assume it's part of the UI.
            // For now we'll just populate what we can from the request/DB.
            
            return new SSProjectSolution.Response.StockManagementReportDto
            {
                Summary = summary,
                StockBalances = balance,
                Transactions = transactions,
                FromDate = request.FromDate?.ToString("yyyy-MM-dd"),
                ToDate = request.ToDate?.ToString("yyyy-MM-dd"),
                CompanyName = request.CompanyId != null ? request.CompanyId.ToString() : "All",
                Branch = "Main Branch"
            };
        }
    }
}
