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
        private readonly IPdfSaveService _pdfSaveService;

        public PrintController(IPdfSaveService pdfSaveService)
        {
            _pdfSaveService = pdfSaveService;
        }

        [HttpPost("save-pdf")]
        public async Task<IActionResult> SavePdf([FromBody] PrintPdfRequest request)
        {
            try
            {
                // Note: The previous IPrintService.SavePdfAsync was removed.
                // Assuming PrintPdfRequest contains Base64Pdf and DcNo.
                if (request == null || string.IsNullOrWhiteSpace(request.Base64Pdf) || string.IsNullOrWhiteSpace(request.DcNo))
                {
                    return BadRequest(new { success = false, message = "Invalid request" });
                }

                byte[] pdfBytes = Convert.FromBase64String(request.Base64Pdf);
                string savedPath = await _pdfSaveService.SavePdfAsync(pdfBytes, request.DcNo);

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

        [HttpPost("generate-dc")]
        public async Task<IActionResult> GenerateDeliveryChallan(
            [FromServices] IPrintWorkflowService workflowService, 
            [FromBody] GenerateDcRequest request)
        {
            try
            {
                var response = await workflowService.GenerateAndPrintDcAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
