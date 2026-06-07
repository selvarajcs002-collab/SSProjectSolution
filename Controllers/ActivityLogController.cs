using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Services;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityLogController : ControllerBase
    {
        private readonly IActivityLogService _service;

        public ActivityLogController(IActivityLogService service)
        {
            _service = service;
        }

        [HttpPost("get-logs")]
        public async Task<IActionResult> GetActivityLogs([FromBody] ActivityLogRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request payload" });
            }

            var result = await _service.GetActivityLogAsync(request);

            return Ok(new
            {
                success = true,
                data = result.Data,
                totalRecords = result.TotalRecords,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                summary = result.Summary
            });
        }

        [HttpPost("advanced-filter")]
        public async Task<IActionResult> AdvancedFilter([FromBody] ActivityLogRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = "Invalid request payload" });
            }

            var result = await _service.AdvancedFilterAsync(request);

            return Ok(new
            {
                success = true,
                data = result.Data,
                totalRecords = result.TotalRecords,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize,
                totalPages = result.TotalPages,
                summary = result.Summary
            });
        }
    }
}
