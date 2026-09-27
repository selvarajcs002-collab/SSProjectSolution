using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SSProjectSolution.Exceptions;
using SSProjectSolution.Request;
using SSProjectSolution.Services;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvanceAmountController : ControllerBase
    {
        private readonly IAdvanceAmountService _service;
        private readonly ILogger<AdvanceAmountController> _logger;

        public AdvanceAmountController(IAdvanceAmountService service, ILogger<AdvanceAmountController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _service.GetAllAdvancesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _service.GetAdvanceByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddAdvanceAmountDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (id, challanNo, challanFileName) = await _service.AddAdvanceAsync(request);
                
                return Ok(new 
                { 
                    success = true, 
                    message = "Advance amount created successfully.", 
                    advanceId = id,
                    challanNo = challanNo,
                    challanFileName = challanFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAdvanceAmountDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, challanNo, challanFileName) = await _service.UpdateAdvanceAsync(request);
                
                if (!success)
                    return NotFound("Advance amount not found");

                return Ok(new 
                { 
                    success = true, 
                    message = "Advance amount updated successfully.",
                    advanceId = request.Id,
                    challanNo = challanNo,
                    challanFileName = challanFileName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAdvanceAsync(id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("GenerateChallan")]
        public async Task<IActionResult> GenerateChallan([FromBody] GenerateAdvanceChallanRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Advance challan request is required." });

            try
            {
                var generatedBy = string.IsNullOrWhiteSpace(User?.Identity?.Name) ? "Admin" : User.Identity!.Name!;
                var file = await _service.GenerateChallanAsync(request, generatedBy);
                return File(file.PdfBytes, "application/pdf", file.FileName);
            }
            catch (AdvanceChallanValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (AdvanceAmountNotFoundException)
            {
                return NotFound(new { message = "Advance amount record not found." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate advance challan for AdvanceId {AdvanceId}", request.AdvanceId);
                return StatusCode(500, new { message = "Unable to generate advance challan." });
            }
        }

        [HttpPost("Export")]
        public async Task<IActionResult> Export([FromBody] ExportAdvanceAmountRequest request)
        {
            try
            {
                var generatedBy = User.Identity?.Name ?? "Admin";
                var fileBytes = await _service.GenerateExportExcelAsync(request.FromDate, request.ToDate, generatedBy);
                var fileName = $"Advance_Amount_Report_{request.FromDate:yyyyMMdd}_to_{request.ToDate:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
