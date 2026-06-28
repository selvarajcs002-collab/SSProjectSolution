using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Services;
using System;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelReportController : ControllerBase
    {
        private readonly IExcelReportService _excelReportService;

        public ExcelReportController(IExcelReportService excelReportService)
        {
            _excelReportService = excelReportService;
        }

        [HttpPost("delivery-challan")]
        public async Task<IActionResult> GetDeliveryChallanReport([FromBody] ReportFilterRequestDto request)
        {
            try
            {
                var response = await _excelReportService.GetDeliveryChallanReportAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                // In a real production scenario, use ILogger to log this error
                return StatusCode(500, new { message = "An error occurred while generating the report", details = ex.Message });
            }
        }
    }
}
