using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SSProjectSolution.Services;
using System;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveryChallanController : ControllerBase
    {
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IDeliveryChallanService _deliveryChallanService;
        private readonly IInwardService _inwardService;

        public DeliveryChallanController(
            IPdfGenerator pdfGenerator,
            IDeliveryChallanService deliveryChallanService,
            IInwardService inwardService)
        {
            _pdfGenerator = pdfGenerator;
            _deliveryChallanService = deliveryChallanService;
            _inwardService = inwardService;
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }

        [HttpDelete("delete-inward/{id}")]
        public async Task<IActionResult> DeleteInward(int id)
        {
            try
            {
                var response = await _inwardService.DeleteInwardAsync(id);

                if (response.Status)
                {
                    return Ok(response);
                }
                
                return BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SSProjectSolution.Response.CommonResponse
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("GenerateAndDownloadDC")]
        public async Task<IActionResult> GenerateAndDownloadDC([FromBody] JObject payload)
        {
            try
            {
                if (payload == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request payload" });
                }

                byte[] pdfBytes = await _pdfGenerator.GeneratePdfAsync(payload);
                
                string dcNo = payload.Value<string>("dcNo") ?? payload.Value<string>("DcNo") ?? $"DC_{DateTime.Now:yyyyMMddHHmmss}";
                string fileName = $"{dcNo}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Failed to generate PDF: " + ex.Message });
            }
        }

        [HttpPost("SaveAndPrintDC")]
        public async Task<IActionResult> SaveAndPrintDC([FromBody] JObject payload)
        {
            try
            {
                if (payload == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request payload" });
                }

                var response = await _deliveryChallanService.ProcessSaveAndPrintAsync(payload);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(500, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    ErrorCode = ex.GetType().Name
                });
            }
        }
    }
}
