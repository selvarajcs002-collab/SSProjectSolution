using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SSProjectSolution.Request;
using SSProjectSolution.Services;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api")]
    public class DcFilterController : ControllerBase
    {
        private readonly IDcFilterService _service;

        public DcFilterController(IDcFilterService service)
        {
            _service = service;
        }

        [HttpPost("get-details")]
        public async Task<IActionResult> GetDetails([FromBody] InwardOutwardFilterRequest request)
        {
            var result = await _service.GetInwardOutwardDetailsAsync(request);

            return Ok(new
            {
                success = true,
                data = result
            });
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string mode,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _service.GetInwardOutwardAsync(mode, pageNumber, pageSize);

            return Ok(new
            {
                success = true,
                data = result,
                pageNumber,
                pageSize
            });
        }
    }
}
