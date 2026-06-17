using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSProjectSolution.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DcDetailController : ControllerBase
    {
        private readonly IDcDetailService _service;

        public DcDetailController(IDcDetailService service)
        {
            _service = service;
        }

        // GET /api/DcDetail/inward-dcs/{companyId}?styleNo=...&designName=...
        [HttpGet("inward-dcs/{companyId}")]
        public async Task<IActionResult> GetDcNumbersByCompany(
            int companyId,
            [FromQuery] string styleNo = null,
            [FromQuery] string designName = null)
        {
            try
            {
                var result = await _service.GetDcNumbersByCompanyAsync(companyId, styleNo, designName);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Returns inward details for one or multiple DC numbers.
        ///
        /// Single DC (original, unchanged behaviour):
        ///   GET /api/DcDetail/inward-details?companyId=1&amp;inwardDcNo=DC_001
        ///
        /// Multiple DCs (new – repeat the query param):
        ///   GET /api/DcDetail/inward-details?companyId=1&amp;inwardDcNo=DC_001&amp;inwardDcNo=DC_002
        ///
        /// The response shape is identical in both cases.
        /// </summary>
        // GET /api/DcDetail/inward-details?companyId=1&inwardDcNo=DC_001[&inwardDcNo=DC_002...]
        [HttpGet("inward-details")]
        public async Task<IActionResult> GetInwardDetailsByDc(
            [FromQuery] int companyId,
            [FromQuery] List<string> inwardDcNo,
            [FromQuery] string colour = null)   // ASP.NET Core binds repeated params automatically
        {
            try
            {
                // Validate
                if (companyId <= 0)
                    return BadRequest(new { success = false, message = "companyId must be a positive integer." });

                var dcNos = inwardDcNo
                    .Where(d => !string.IsNullOrWhiteSpace(d))
                    .Select(d => d.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (dcNos.Count == 0)
                    return BadRequest(new { success = false, message = "At least one inwardDcNo must be supplied." });

                var result = await _service.GetInwardDetailsByDcsAsync(companyId, dcNos, colour);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
