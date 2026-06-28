using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Response;
using SSProjectSolution.Services;

namespace SSProjectSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Uncomment when authorization is configured
    public class RateQuotationController : ControllerBase
    {
        private readonly IRateQuotationService _service;

        public RateQuotationController(IRateQuotationService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<long>>> CreateAsync([FromBody] RateQuotationCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<long>.ErrorResponse("Validation Failed", errors));
            }

            var response = await _service.CreateAsync(createDto);

            if (response.Success)
            {
                return CreatedAtAction(nameof(GetByIdAsync).Replace("Async", ""), new { id = response.Data }, response);
            }

            // You can map to specific status codes based on response.Message if needed
            if (response.Message.Contains("already exists"))
            {
                return Conflict(response);
            }

            return BadRequest(response);
        }

        [HttpPut("update/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateAsync(long id, [FromBody] RateQuotationUpdateDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse("Validation Failed", errors));
            }

            var response = await _service.UpdateAsync(id, updateDto);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(long id, [FromQuery] long modifiedBy)
        {
            var response = await _service.DeleteAsync(id, modifiedBy);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpGet("getbyid/{id}")]
        public async Task<ActionResult<ApiResponse<RateQuotationResponseDto>>> GetByIdAsync(long id)
        {
            var response = await _service.GetByIdAsync(id);

            if (response.Success)
            {
                return Ok(response);
            }

            if (response.Message.Contains("not found"))
            {
                return NotFound(response);
            }

            return BadRequest(response);
        }

        [HttpGet("getall")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RateQuotationResponseDto>>>> GetAllAsync()
        {
            var response = await _service.GetAllAsync();

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("search")]
        public async Task<ActionResult<ApiResponse<IEnumerable<RateQuotationResponseDto>>>> SearchAsync([FromBody] RateQuotationSearchDto searchDto)
        {
            var response = await _service.SearchAsync(searchDto);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("paged")]
        public async Task<ActionResult<ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>>> GetPagedAsync([FromBody] RateQuotationSearchDto searchDto)
        {
            var response = await _service.GetPagedAsync(searchDto);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
