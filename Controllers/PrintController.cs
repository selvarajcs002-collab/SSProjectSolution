using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Request;
using SSProjectSolution.Services;
using System.Threading.Tasks;
using System;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintController : ControllerBase
    {
        private readonly IPrintService _printService;

        public PrintController(IPrintService printService)
        {
            _printService = printService;
        }

        [HttpPost("save-pdf")]
        public async Task<IActionResult> SavePdf([FromBody] PrintPdfRequest request)
        {
            try
            {
                string savedPath = await _printService.SavePdfAsync(request);
                return Ok(new { 
                    success = true, 
                    message = $"PDF saved successfully to {savedPath}", 
                    path = savedPath 
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
