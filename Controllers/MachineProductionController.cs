using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Data;
using SSProjectSolution.Data;
using SSProjectSolution.Models.DTOs;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Style;
using System.Drawing;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MachineProductionController : ControllerBase
    {
        private readonly DapperDBConnection _dbConnection;

        public MachineProductionController(DapperDBConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProduction([FromBody] MachineProductionDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Invalid production data" });

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeName", model.EmployeeName);
                parameters.Add("@MachineName", model.MachineName);
                parameters.Add("@Shift", model.Shift);
                parameters.Add("@StyleName", model.StyleName);
                parameters.Add("@DesignName", model.DesignName);
                parameters.Add("@TotalProduction", model.TotalProduction);
                parameters.Add("@TargetProduction", model.TargetProduction);
                parameters.Add("@CostPerPiece", model.CostPerPiece);
                parameters.Add("@ProductionCost", model.ProductionCost);
                parameters.Add("@Status", model.Status);
                parameters.Add("@CompanyId", model.CompanyId);

                var result = await connection.QueryAsync<int>(
                    "sp_EMP_AddDailyProduction",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var newId = result.FirstOrDefault();
                return Ok(new { success = true, id = newId, message = "Production entry saved successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateProduction(int id, [FromBody] MachineProductionDto model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { success = false, message = "Invalid production data" });

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);
                parameters.Add("@EmployeeName", model.EmployeeName);
                parameters.Add("@MachineName", model.MachineName);
                parameters.Add("@Shift", model.Shift);
                parameters.Add("@StyleName", model.StyleName);
                parameters.Add("@DesignName", model.DesignName);
                parameters.Add("@TotalProduction", model.TotalProduction);
                parameters.Add("@TargetProduction", model.TargetProduction);
                parameters.Add("@CostPerPiece", model.CostPerPiece);
                parameters.Add("@ProductionCost", model.ProductionCost);
                parameters.Add("@Status", model.Status);
                parameters.Add("@CompanyId", model.CompanyId);

                var result = await connection.QueryAsync<int>(
                    "sp_EMP_UpdateDailyProduction",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var updatedId = result.FirstOrDefault();
                if (updatedId <= 0)
                    return NotFound(new { success = false, message = "Production record not found" });

                return Ok(new { success = true, id = updatedId, message = "Production entry updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduction(int id)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", id);

                var result = await connection.QueryAsync<int>(
                    "sp_EMP_DeleteDailyProduction",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var deletedId = result.FirstOrDefault();
                if (deletedId <= 0)
                    return NotFound(new { success = false, message = "Production record not found" });

                return Ok(new { success = true, id = deletedId, message = "Production entry deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("list/{companyId}")]
        public async Task<IActionResult> GetProductionList(int companyId, [FromQuery] string? shift = null)
        {
            try
            {
                if (companyId <= 0)
                    return BadRequest(new { message = "Invalid CompanyId" });

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@CompanyId", companyId);
                parameters.Add("@Shift", shift);

                var records = await connection.QueryAsync<MachineProductionDto>(
                    "sp_EMP_GetDailyProductionsByCompany",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("paginated-list")]
        public async Task<IActionResult> GetPaginatedProductionList([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? shift = null)
        {
            try
            {
                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Shift", shift);

                // Assuming sp_EMP_GetAllDailyProductions is created to fetch all records
                var records = await connection.QueryAsync<MachineProductionDto>(
                    "sp_EMP_GetAllDailyProductions",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // Order by latest entry first (using Id descending) and paginate
                var paginatedRecords = records
                    .OrderByDescending(x => x.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalRecords = records.Count();

                return Ok(new
                {
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    CurrentPage = page,
                    PageSize = pageSize,
                    Data = paginatedRecords
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportExcel([FromBody] ExportProductionRequestDto request)
        {
            try
            {
                if (request == null || request.FromDate > request.ToDate)
                {
                    return BadRequest("Invalid date range.");
                }

                using var connection = _dbConnection.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@FromDate", request.FromDate);
                parameters.Add("@ToDate", request.ToDate);

                var records = await connection.QueryAsync<MachineProductionDto>(
                    "sp_EMP_GetProductionExportData",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var recordList = records.ToList();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Production Log Report");

                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;

                var primaryBlue = Color.FromArgb(11, 59, 140);
                var accentGreen = Color.FromArgb(14, 124, 58);
                var kpiBg1 = Color.FromArgb(244, 247, 252);
                var kpiBg2 = Color.FromArgb(250, 245, 255);
                var kpiBg3 = Color.FromArgb(235, 248, 255);
                var kpiBg4 = Color.FromArgb(255, 245, 235);
                var kpiBg5 = Color.FromArgb(255, 240, 245);
                var kpiBg6 = Color.FromArgb(235, 255, 240);

                var logoPath = @"C:\Users\ADMIN\Desktop\SSProject\SSMangementUI-dev\SSMangementUI-dev\public\logo.jpg";
                if (System.IO.File.Exists(logoPath))
                {
                    var logo = worksheet.Drawings.AddPicture("Logo", new FileInfo(logoPath));
                    logo.SetPosition(0, 5, 0, 5); 
                    logo.SetSize(180, 70);
                }

                worksheet.Cells["D1:H2"].Merge = true;
                worksheet.Cells["D1:H2"].Value = "S.S. MANAGEMENT";
                worksheet.Cells["D1:H2"].Style.Font.Bold = true;
                worksheet.Cells["D1:H2"].Style.Font.Size = 20;
                worksheet.Cells["D1:H2"].Style.Font.Color.SetColor(primaryBlue);
                worksheet.Cells["D1:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["D1:H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Cells["D3:H3"].Merge = true;
                worksheet.Cells["D3:H3"].Value = "PRODUCTION LOG REPORT";
                worksheet.Cells["D3:H3"].Style.Font.Bold = true;
                worksheet.Cells["D3:H3"].Style.Font.Size = 14;
                worksheet.Cells["D3:H3"].Style.Font.Color.SetColor(accentGreen);
                worksheet.Cells["D3:H3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["J1"].Value = "Generated On";
                worksheet.Cells["K1"].Value = ": " + DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
                worksheet.Cells["J2"].Value = "Generated By";
                worksheet.Cells["K2"].Value = ": " + (User.Identity?.Name ?? "Admin");
                worksheet.Cells["J3"].Value = "Company";
                worksheet.Cells["K3"].Value = ": 1 - Main Company";
                worksheet.Cells["J4"].Value = "Branch";
                worksheet.Cells["K4"].Value = ": Main Branch";
                
                worksheet.Cells["C5:D5"].Merge = true;
                worksheet.Cells["C5:D5"].Value = "From Date :   " + request.FromDate.ToString("dd-MMM-yyyy");
                worksheet.Cells["C5:D5"].Style.Font.Bold = true;
                worksheet.Cells["C5:D5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["C5:D5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                worksheet.Cells["C5:D5"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                worksheet.Cells["G5:H5"].Merge = true;
                worksheet.Cells["G5:H5"].Value = "To Date :   " + request.ToDate.ToString("dd-MMM-yyyy");
                worksheet.Cells["G5:H5"].Style.Font.Bold = true;
                worksheet.Cells["G5:H5"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["G5:H5"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 230, 241));
                worksheet.Cells["G5:H5"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                var today = DateTime.Today;
                var totalDayProduction = recordList.Where(x => x.Shift == "Day").Sum(x => x.TotalProduction);
                var totalNightProduction = recordList.Where(x => x.Shift == "Night").Sum(x => x.TotalProduction);
                var totalProduction = recordList.Sum(x => x.TotalProduction);
                var totalDayCost = recordList.Where(x => x.Shift == "Day").Sum(x => x.ProductionCost);
                var totalNightCost = recordList.Where(x => x.Shift == "Night").Sum(x => x.ProductionCost);
                var totalRunningCost = totalDayCost + totalNightCost;

                void DrawKPI(int col, string title, decimal numVal, string format, Color bgColor, Color textColor)
                {
                    worksheet.Cells[7, col, 7, col+1].Merge = true;
                    worksheet.Cells[7, col].Value = title;
                    worksheet.Cells[7, col].Style.Font.Bold = true;
                    worksheet.Cells[7, col].Style.Font.Size = 9;
                    worksheet.Cells[7, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[7, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    
                    worksheet.Cells[8, col, 8, col+1].Merge = true;
                    worksheet.Cells[8, col].Value = numVal;
                    worksheet.Cells[8, col].Style.Numberformat.Format = format;
                    worksheet.Cells[8, col].Style.Font.Bold = true;
                    worksheet.Cells[8, col].Style.Font.Size = 14;
                    worksheet.Cells[8, col].Style.Font.Color.SetColor(textColor);
                    worksheet.Cells[8, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[8, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    var range = worksheet.Cells[7, col, 8, col+1];
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(bgColor);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.LightGray);
                }

                DrawKPI(1, "☀️ TOTAL DAY PRODUCTION", totalDayProduction, "#,##0", kpiBg1, primaryBlue);
                DrawKPI(3, "🌙 TOTAL NIGHT PRODUCTION", totalNightProduction, "#,##0", kpiBg2, Color.Purple);
                DrawKPI(5, "📦 TOTAL PRODUCTION", totalProduction, "#,##0", kpiBg3, primaryBlue);
                DrawKPI(7, "💰 TOTAL DAY PRODUCTION COST", totalDayCost, "[$₹-en-IN] #,##0.00", kpiBg4, accentGreen);
                DrawKPI(9, "💰 TOTAL NIGHT PRODUCTION COST", totalNightCost, "[$₹-en-IN] #,##0.00", kpiBg5, Color.Purple);
                DrawKPI(11, "💰 TOTAL RUNNING COST", totalRunningCost, "[$₹-en-IN] #,##0.00", kpiBg6, accentGreen);

                int headerRow = 10;
                worksheet.Cells[headerRow - 1, 1, headerRow - 1, 11].Merge = true;
                worksheet.Cells[headerRow - 1, 1].Value = "PRODUCTION LOG DETAILS";
                worksheet.Cells[headerRow - 1, 1].Style.Font.Bold = true;
                worksheet.Cells[headerRow - 1, 1].Style.Font.Color.SetColor(Color.White);
                worksheet.Cells[headerRow - 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[headerRow - 1, 1].Style.Fill.BackgroundColor.SetColor(primaryBlue);

                string[] headers = { "S.No", "Date", "Shift", "Machine", "Operator Name", "Style", "Design", "Total Production", "Target Production", "Cost Per Piece", "Production Cost" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[headerRow, i + 1].Value = headers[i];
                }
                var headerRange = worksheet.Cells[headerRow, 1, headerRow, headers.Length];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 198, 231));
                headerRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                headerRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                int row = headerRow + 1;
                int sno = 1;
                foreach (var r in recordList)
                {
                    worksheet.Cells[row, 1].Value = sno++;
                    if (r.CreatedDate.HasValue) {
                        worksheet.Cells[row, 2].Value = r.CreatedDate.Value.Date;
                    }
                    worksheet.Cells[row, 3].Value = r.Shift;

                    string machName = r.MachineName ?? "";
                    var match = System.Text.RegularExpressions.Regex.Match(machName, @"\d+");
                    if (match.Success) {
                        worksheet.Cells[row, 4].Value = "Machine " + int.Parse(match.Value);
                    } else {
                        worksheet.Cells[row, 4].Value = machName;
                    }

                    worksheet.Cells[row, 5].Value = r.EmployeeName;
                    worksheet.Cells[row, 6].Value = r.StyleName;
                    worksheet.Cells[row, 7].Value = r.DesignName;
                    worksheet.Cells[row, 8].Value = r.TotalProduction;
                    worksheet.Cells[row, 9].Value = r.TargetProduction;
                    worksheet.Cells[row, 10].Value = r.CostPerPiece;
                    worksheet.Cells[row, 11].Value = r.ProductionCost;
                    row++;
                }

                if (recordList.Any())
                {
                    var dataRange = worksheet.Cells[headerRow, 1, row - 1, headers.Length];
                    var table = worksheet.Tables.Add(dataRange, "ProductionTable");
                    table.ShowFilter = true;
                    table.TableStyle = OfficeOpenXml.Table.TableStyles.Light8;

                    var dataCells = worksheet.Cells[headerRow + 1, 1, row - 1, headers.Length];
                    dataCells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    dataCells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    dataCells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    dataCells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    dataCells.Style.Border.Top.Color.SetColor(Color.LightGray);
                    dataCells.Style.Border.Bottom.Color.SetColor(Color.LightGray);
                    dataCells.Style.Border.Left.Color.SetColor(Color.LightGray);
                    dataCells.Style.Border.Right.Color.SetColor(Color.LightGray);
                }

                worksheet.Column(1).Width = 8;
                worksheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                worksheet.Column(2).Width = 15;
                worksheet.Column(2).Style.Numberformat.Format = "dd-MMM-yyyy";
                worksheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                worksheet.Column(3).Width = 12;
                worksheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                worksheet.Column(4).Width = 15;
                worksheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                worksheet.Column(5).Width = 22;
                worksheet.Column(6).Width = 18;
                worksheet.Column(7).Width = 22;
                
                worksheet.Column(8).Width = 18;
                worksheet.Column(8).Style.Numberformat.Format = "#,##0";
                worksheet.Column(9).Width = 18;
                worksheet.Column(9).Style.Numberformat.Format = "#,##0";
                
                worksheet.Column(10).Width = 18;
                worksheet.Column(10).Style.Numberformat.Format = "[$₹-en-IN] #,##0.00";
                worksheet.Column(11).Width = 20;
                worksheet.Column(11).Style.Numberformat.Format = "[$₹-en-IN] #,##0.00";

                worksheet.View.FreezePanes(headerRow + 1, 1);

                if (recordList.Any())
                {
                    worksheet.Cells[row, 1, row, 7].Merge = true;
                    worksheet.Cells[row, 1].Value = "Total";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    worksheet.Cells[row, 8].Formula = $"SUBTOTAL(109, H{headerRow + 1}:H{row - 1})";
                    worksheet.Cells[row, 9].Formula = $"SUBTOTAL(109, I{headerRow + 1}:I{row - 1})";
                    worksheet.Cells[row, 11].Formula = $"SUBTOTAL(109, K{headerRow + 1}:K{row - 1})";
                    
                    worksheet.Cells[row, 10].Formula = $"IF(SUBTOTAL(109, H{headerRow + 1}:H{row - 1})=0, 0, SUBTOTAL(109, K{headerRow + 1}:K{row - 1})/SUBTOTAL(109, H{headerRow + 1}:H{row - 1}))";
                    worksheet.Cells[row, 10].Style.Numberformat.Format = "[$₹-en-IN] #,##0.00";
                    
                    var totalRange = worksheet.Cells[row, 1, row, headers.Length];
                    totalRange.Style.Font.Bold = true;
                    totalRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    totalRange.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(189, 215, 238));
                }

                if (recordList.Any())
                {
                    int chartRow = row + 2;
                    
                    var helperWs = package.Workbook.Worksheets.Add("ChartData");
                    helperWs.Hidden = eWorkSheetHidden.Hidden;
                    
                    helperWs.Cells["A1"].Value = "Metric";
                    helperWs.Cells["B1"].Value = "Day";
                    helperWs.Cells["C1"].Value = "Night";
                    
                    helperWs.Cells["A2"].Value = "Production";
                    helperWs.Cells["B2"].Value = recordList.Where(x => x.Shift == "Day").Sum(x => x.TotalProduction);
                    helperWs.Cells["C2"].Value = totalNightProduction; 
                    
                    helperWs.Cells["A3"].Value = "Cost";
                    helperWs.Cells["B3"].Value = totalDayCost;
                    helperWs.Cells["C3"].Value = totalNightCost;
                    
                    helperWs.Cells["A4"].Value = "Count";
                    helperWs.Cells["B4"].Value = recordList.Count(x => x.Shift == "Day");
                    helperWs.Cells["C4"].Value = recordList.Count(x => x.Shift == "Night");

                    var chart1 = worksheet.Drawings.AddChart("Chart_Production", eChartType.ColumnClustered);
                    chart1.Title.Text = "Production - Day vs Night";
                    chart1.SetPosition(chartRow, 0, 0, 0);
                    chart1.SetSize(350, 250);
                    var series1Day = chart1.Series.Add(helperWs.Cells["B2:B2"], helperWs.Cells["B1:B1"]);
                    series1Day.Header = "Day";
                    var series1Night = chart1.Series.Add(helperWs.Cells["C2:C2"], helperWs.Cells["C1:C1"]);
                    series1Night.Header = "Night";

                    var chart2 = worksheet.Drawings.AddChart("Chart_Cost", eChartType.ColumnClustered);
                    chart2.Title.Text = "Production Cost - Day vs Night";
                    chart2.SetPosition(chartRow, 0, 4, 0);
                    chart2.SetSize(350, 250);
                    var series2Day = chart2.Series.Add(helperWs.Cells["B3:B3"], helperWs.Cells["B1:B1"]);
                    series2Day.Header = "Day Cost";
                    var series2Night = chart2.Series.Add(helperWs.Cells["C3:C3"], helperWs.Cells["C1:C1"]);
                    series2Night.Header = "Night Cost";

                    var chart3 = worksheet.Drawings.AddChart("Chart_Count", eChartType.ColumnClustered);
                    chart3.Title.Text = "Production Count - Day vs Night";
                    chart3.SetPosition(chartRow, 0, 8, 0);
                    chart3.SetSize(350, 250);
                    var series3Day = chart3.Series.Add(helperWs.Cells["B4:B4"], helperWs.Cells["B1:B1"]);
                    series3Day.Header = "Day Count";
                    var series3Night = chart3.Series.Add(helperWs.Cells["C4:C4"], helperWs.Cells["C1:C1"]);
                    series3Night.Header = "Night Count";
                }

                var content = package.GetAsByteArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = $"Production_Log_Report_{request.FromDate:yyyy-MM-dd}_to_{request.ToDate:yyyy-MM-dd}.xlsx";
                
                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error generating Excel: " + ex.Message });
            }
        }
    }
}

