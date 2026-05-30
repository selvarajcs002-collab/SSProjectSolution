using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/outward")]
    public class OutwardController : ControllerBase
    {
        private readonly IOutwardService _outwardService;

        public OutwardController(IOutwardService outwardService)
        {
            _outwardService = outwardService;
        }

        // ── Size-Based (existing — untouched) ──────────────────────────────────

        [HttpPost("save-outward")]
        public async Task<IActionResult> SaveOutward([FromBody] OutwardRequest request)
        {
            try
            {
                // VALIDATIONS
                if (request == null || request.Outward == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request payload" });
                }

                if (string.IsNullOrEmpty(request.Outward.Mode) || 
                   (request.Outward.Mode != "INSERT" && request.Outward.Mode != "UPDATE"))
                {
                    return BadRequest(new { success = false, message = "Mode must be INSERT or UPDATE" });
                }

                if (request.Sizes == null || request.Sizes.Count == 0)
                {
                    return BadRequest(new { success = false, message = "Sizes should not be empty" });
                }

                if (request.Outward.CompanyId <= 0)
                {
                    return BadRequest(new { success = false, message = "CompanyId must be valid" });
                }

                var response = await _outwardService.SaveOutwardAsync(request);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error: " + ex.Message });
            }
        }

        [HttpGet("outward_get_by_dcno")]
        public async Task<IActionResult> GetOutwardByDcNo([FromQuery] int id, [FromQuery] string mode)
        {
            try
            {
                if (id <= 0 || string.IsNullOrEmpty(mode))
                {
                    return BadRequest(new { success = false, message = "Valid Id and Mode are required" });
                }

                var response = await _outwardService.GetOutwardByDcNoAsync(id, mode);

                if (response == null)
                {
                    return NotFound(new { success = false, message = "Outward record not found" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error: " + ex.Message });
            }
        }

        [HttpPost("outward-update")]
        public async Task<IActionResult> UpdateOutward([FromBody] OutwardUpdateRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request payload" });
                }

                if (request.OutwardId <= 0)
                {
                    return BadRequest(new { success = false, message = "OutwardId must be valid for update" });
                }

                if (request.SizeCounts == null || request.SizeCounts.Count == 0)
                {
                    return BadRequest(new { success = false, message = "SizeCounts should not be empty" });
                }

                var response = await _outwardService.UpdateOutwardAsync(request);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response); // Use 400 for business logic failure
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error: " + ex.Message });
            }
        }

        // ── Meter-Based (new — isolated) ───────────────────────────────────────

        [HttpPost("save-meter-outward")]
        public async Task<IActionResult> SaveMeterOutward([FromBody] OutwardMeterSaveRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { success = false, message = "Invalid request payload" });

                if (request.MeterDetails == null || !request.MeterDetails.Any())
                    return BadRequest(new { success = false, message = "MeterDetails cannot be empty" });

                if (request.CompanyId <= 0)
                    return BadRequest(new { success = false, message = "CompanyId must be valid" });

                // Detect duplicate meter values in request
                var duplicates = request.MeterDetails
                    .GroupBy(m => m.MeterValue)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Duplicate MeterValue entries detected: {string.Join(", ", duplicates)}. Each meter value must be unique per outward."
                    });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );
                    return BadRequest(new { success = false, message = "Validation Failed", errors });
                }

                var response = await _outwardService.SaveMeterOutwardAsync(request);

                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal Server Error: " + ex.Message });
            }
        }
    }
}
