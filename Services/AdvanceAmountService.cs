using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SSProjectSolution.Repositories;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public class AdvanceAmountService : IAdvanceAmountService
    {
        private readonly IAdvanceAmountRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly IAdvanceChallanService _challanService;

        public AdvanceAmountService(IAdvanceAmountRepository repository, IConfiguration configuration, IAdvanceChallanService challanService)
        {
            _repository = repository;
            _configuration = configuration;
            _challanService = challanService;
        }

        public async Task<IEnumerable<AdvanceAmountDto>> GetAllAdvancesAsync()
        {
            return await _repository.GetAllAdvancesAsync();
        }

        public async Task<AdvanceAmountDto> GetAdvanceByIdAsync(int id)
        {
            return await _repository.GetAdvanceByIdAsync(id);
        }

        public async Task<(int Id, string ChallanNo, string ChallanFileName)> AddAdvanceAsync(AddAdvanceAmountDto dto)
        {
            var id = await _repository.AddAdvanceAsync(dto);
            
            var advance = new SSProjectSolution.Models.AdvanceAmount 
            { 
                Id = id, 
                Name = dto.Name, 
                Date = dto.Date, 
                Amount = dto.Amount, 
                Remarks = dto.Remarks 
            };
            
            var (challanNo, challanFileName) = await _challanService.GenerateAndSaveChallanAsync(advance);
            
            await _repository.UpdateAdvanceChallanAsync(id, challanNo, challanFileName);
            
            return (id, challanNo, challanFileName);
        }

        public async Task<(bool Success, string ChallanNo, string ChallanFileName)> UpdateAdvanceAsync(UpdateAdvanceAmountDto dto)
        {
            var success = await _repository.UpdateAdvanceAsync(dto);
            if (!success) return (false, string.Empty, string.Empty);
            
            var existingRecord = await _repository.GetAdvanceByIdAsync(dto.Id);
            
            var advance = new SSProjectSolution.Models.AdvanceAmount 
            { 
                Id = dto.Id, 
                Name = dto.Name, 
                Date = dto.Date, 
                Amount = dto.Amount, 
                Remarks = dto.Remarks,
                ChallanNo = existingRecord?.ChallanNo
            };
            
            var (challanNo, challanFileName) = await _challanService.GenerateAndSaveChallanAsync(advance);
            
            await _repository.UpdateAdvanceChallanAsync(dto.Id, challanNo, challanFileName);
            
            return (true, challanNo, challanFileName);
        }

        public async Task<bool> DeleteAdvanceAsync(int id)
        {
            return await _repository.DeleteAdvanceAsync(id);
        }

        public async Task<byte[]> GenerateExportExcelAsync(DateTime fromDate, DateTime toDate, string generatedBy)
        {
            var data = (await _repository.GetAdvancesForExportAsync(fromDate, toDate)).ToList();
            
            var companyName = _configuration["CompanySettings:CompanyName"] ?? "S.S. EMBROIDERY";
            var logoPath = _configuration["CompanySettings:LogoPath"];

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Advance Amount Report");

            // Page Setup
            worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
            worksheet.PrinterSettings.PaperSize = ePaperSize.A4;
            worksheet.PrinterSettings.FitToPage = true;
            worksheet.PrinterSettings.FitToWidth = 1;
            worksheet.PrinterSettings.FitToHeight = 0;
            worksheet.PrinterSettings.RepeatRows = new ExcelAddress("1:16");

            // Header Section
            worksheet.Cells["A1:E1"].Merge = true;
            worksheet.Cells["A1"].Value = companyName;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells["A2:E2"].Merge = true;
            worksheet.Cells["A2"].Value = "ADVANCE AMOUNT REPORT";
            worksheet.Cells["A2"].Style.Font.Bold = true;
            worksheet.Cells["A2"].Style.Font.Size = 14;
            worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            // Optional Logo
            if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
            {
                try
                {
                    var logo = worksheet.Drawings.AddPicture("Logo", new FileInfo(logoPath));
                    logo.SetPosition(0, 5, 0, 5);
                    logo.SetSize(100, 50);
                }
                catch { } // Ignore if logo fails to load
            }

            var now = DateTime.Now;

            // Info Section
            worksheet.Cells["A4"].Value = "Company";
            worksheet.Cells["B4"].Value = $": {companyName}";
            worksheet.Cells["A5"].Value = "From Date";
            worksheet.Cells["B5"].Value = $": {fromDate:dd-MM-yyyy}";
            worksheet.Cells["A6"].Value = "To Date";
            worksheet.Cells["B6"].Value = $": {toDate:dd-MM-yyyy}";
            worksheet.Cells["A7"].Value = "Generated On";
            worksheet.Cells["B7"].Value = $": {now:dd-MM-yyyy}";
            worksheet.Cells["A8"].Value = "Generated Time";
            worksheet.Cells["B8"].Value = $": {now:HH:mm}";
            worksheet.Cells["A9"].Value = "Generated By";
            worksheet.Cells["B9"].Value = $": {generatedBy}";

            worksheet.Cells["A4:A9"].Style.Font.Bold = true;

            // Summary Section
            worksheet.Cells["A11:E11"].Merge = true;
            worksheet.Cells["A11"].Value = "ADVANCE AMOUNT SUMMARY";
            worksheet.Cells["A11"].Style.Font.Bold = true;
            worksheet.Cells["A11"].Style.Font.Size = 12;
            worksheet.Cells["A11"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A11"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A11"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
            worksheet.Cells["A11"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            worksheet.Cells["A12:B12"].Merge = true;
            worksheet.Cells["A12"].Value = "TOTAL RECORDS";
            worksheet.Cells["A12"].Style.Font.Bold = true;
            
            worksheet.Cells["C12:E12"].Merge = true;
            worksheet.Cells["C12"].Value = "TOTAL ADVANCE AMOUNT";
            worksheet.Cells["C12"].Style.Font.Bold = true;
            
            worksheet.Cells["A13:B13"].Merge = true;
            worksheet.Cells["A13"].Value = data.Count;
            worksheet.Cells["A13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            
            worksheet.Cells["C13:E13"].Merge = true;
            var sumAmount = data.Sum(x => x.Amount);
            worksheet.Cells["C13"].Value = sumAmount;
            worksheet.Cells["C13"].Style.Numberformat.Format = "₹#,##0.00";
            worksheet.Cells["C13"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            worksheet.Cells["A12:E13"].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            // Table Headers
            var startRow = 16;
            worksheet.Cells[startRow, 1].Value = "S.NO";
            worksheet.Cells[startRow, 2].Value = "NAME";
            worksheet.Cells[startRow, 3].Value = "DATE";
            worksheet.Cells[startRow, 4].Value = "AMOUNT (₹)";
            worksheet.Cells[startRow, 5].Value = "REMARKS";

            using (var range = worksheet.Cells[startRow, 1, startRow, 5])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(16, 124, 65)); // Dark green/blue standard Excel theme
                range.Style.Font.Color.SetColor(Color.White);
                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Data Rows
            var row = startRow + 1;
            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                worksheet.Cells[row, 1].Value = i + 1;
                worksheet.Cells[row, 2].Value = item.Name;
                
                worksheet.Cells[row, 3].Value = item.Date;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "dd-MM-yyyy";

                worksheet.Cells[row, 4].Value = item.Amount;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "₹#,##0.00";
                
                worksheet.Cells[row, 5].Value = item.Remarks;
                worksheet.Cells[row, 5].Style.WrapText = true;

                // Alternate row coloring (subtle)
                if (i % 2 == 1)
                {
                    worksheet.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(249, 249, 249));
                }

                // Borders
                using (var range = worksheet.Cells[row, 1, row, 5])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                }

                row++;
            }

            // Alignments
            if (data.Count > 0)
            {
                worksheet.Cells[startRow + 1, 1, row - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[startRow + 1, 2, row - 1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[startRow + 1, 3, row - 1, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[startRow + 1, 4, row - 1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[startRow + 1, 5, row - 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            }

            // Filter
            worksheet.Cells[startRow, 1, row - 1, 5].AutoFilter = true;

            // Total Row
            worksheet.Cells[row, 1, row, 3].Merge = true;
            worksheet.Cells[row, 1].Value = "TOTAL";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            if (data.Count > 0)
            {
                worksheet.Cells[row, 4].Formula = $"SUM(D{startRow + 1}:D{row - 1})";
            }
            else
            {
                worksheet.Cells[row, 4].Value = 0;
            }
            
            worksheet.Cells[row, 4].Style.Font.Bold = true;
            worksheet.Cells[row, 4].Style.Numberformat.Format = "₹#,##0.00";
            worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            
            worksheet.Cells[row, 5].Value = "";

            using (var range = worksheet.Cells[row, 1, row, 5])
            {
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Freeze Panes
            worksheet.View.FreezePanes(startRow + 1, 1);

            // Column Widths
            worksheet.Column(1).Width = 8;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 15;
            worksheet.Column(4).Width = 18;
            worksheet.Column(5).Width = 50;

            return package.GetAsByteArray();
        }

        public Task<AdvanceChallanFileResult> GenerateChallanAsync(GenerateAdvanceChallanRequest request, string generatedBy)
        {
            return _challanService.GenerateChallanAsync(request, generatedBy);
        }
    }
}
