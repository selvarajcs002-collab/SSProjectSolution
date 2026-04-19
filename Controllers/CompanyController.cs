using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Business;
using SSProjectSolution.Models;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/company")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyBusiness _companyBusiness;

        public CompanyController(ICompanyBusiness companyBusiness)
        {
            _companyBusiness = companyBusiness;
        }

        [HttpGet("get-company-list")]
        public async Task<IActionResult> GetCompanyList()
        {
            try
            {
                var result = await _companyBusiness.GetCompanyList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpGet("get-company-by-id/{companyId}")]
        public async Task<IActionResult> GetCompanyById(int companyId)
        {
            try
            {
                var result = await _companyBusiness.GetCompanyById(companyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpPost("save-company")]
        public async Task<IActionResult> SaveCompany([FromBody] CompanyRequest request)
        {
            try
            {
                request.Mode = "INSERT";
                var result = await _companyBusiness.SaveCompany(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpPut("update-company")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyRequest request)
        {
            try
            {
                request.Mode = "UPDATE";

                if (request.CompanyId == null || request.CompanyId == 0)
                {
                    return BadRequest(new CommonResponse { Id = 0, Message = "CompanyId is required for update", Status = false });
                }

                var result = await _companyBusiness.SaveCompany(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }
    }
}
