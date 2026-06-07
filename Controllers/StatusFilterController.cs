using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Services;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusFilterController : ControllerBase
    {
        private readonly IStatusFilterService _service;

        public StatusFilterController(IStatusFilterService service)
        {
            _service = service;
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search([FromBody] StatusFilterRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request payload" });
            }

            var result = await _service.SearchAsync(request);

            if (!result.Success)
            {
                return StatusCode(500, new { success = false, message = result.Message });
            }

            return Ok(new
            {
                success = true,
                message = "",
                totalRecords = result.TotalRecords,
                data = result.Data,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                summary = result.Summary
            });
        }
    }
}
