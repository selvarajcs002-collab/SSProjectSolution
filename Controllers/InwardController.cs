using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Business;
using SSProjectSolution.Services;
using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/inward")]
    public class InwardController : ControllerBase
    {
        private readonly IInwardBusiness _inwardBusiness;
        private readonly IInwardService _inwardService;

        public InwardController(IInwardBusiness inwardBusiness, IInwardService inwardService)
        {
            _inwardBusiness = inwardBusiness;
            _inwardService = inwardService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] InwardSaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _inwardBusiness.SaveInward(request);
                
                if (result.Status)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = "Internal Server Error: " + ex.Message, Status = false });
            }
        }

        [HttpGet("sizes")]
        public async Task<IActionResult> GetSizesByColourStyle([FromQuery] int companyId, [FromQuery] string colour, [FromQuery] string styleNo)
        {
            try
            {
                if (companyId <= 0 || string.IsNullOrEmpty(colour) || string.IsNullOrEmpty(styleNo))
                {
                    return BadRequest(new { message = "Invalid input parameters" });
                }

                var sizes = await _inwardService.GetSizesByColourStyleAsync(companyId, colour, styleNo);
                return Ok(sizes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }

        [HttpGet("by-dc")]
        public async Task<IActionResult> GetInwardByCompanyAndDc([FromQuery] int companyId, [FromQuery] string inwardDcNo)
        {
            try
            {
                if (companyId <= 0 || string.IsNullOrEmpty(inwardDcNo))
                {
                    return BadRequest(new { message = "Invalid input parameters" });
                }

                var inward = await _inwardService.GetInwardByCompanyAndDcAsync(companyId, inwardDcNo);
                if (inward == null)
                {
                    return NotFound(new { message = "Inward not found" });
                }

                return Ok(inward);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateInward([FromBody] InwardUpdateDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var message = await _inwardService.UpdateInwardAsync(request);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }

        [HttpGet("design-style-colour")]
        public async Task<IActionResult> GetDesignStyleColour(int companyId)
        {
            try
            {
                if (companyId <= 0)
                {
                    return BadRequest(new { message = "Invalid CompanyId" });
                }

                var result = await _inwardService.GetDesignStyleColourByCompanyAsync(companyId);

                if (result == null || !result.Any())
                {
                    return NotFound(new { message = "No designs found for this company" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal Server Error: " + ex.Message });
            }
        }
    }
}
