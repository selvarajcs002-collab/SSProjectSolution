using Microsoft.AspNetCore.Mvc;
using SSProjectSolution.Business;
using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly IUserBusiness _userBusiness;

        public LoginController(IUserBusiness userBusiness)
        {
            _userBusiness = userBusiness;
        }

        [HttpPost("save-user")]
        public async Task<IActionResult> SaveUser([FromBody] UserSaveRequest saveRequest)
        {
            try
            {
                var request = new UserRequest
                {
                    Mode = saveRequest.Mode,
                    Email = saveRequest.Email,
                    Password = saveRequest.Password,
                    CreatedBy = User?.Identity?.Name ?? "System"
                };

                var result = await _userBusiness.SaveUser(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UserUpdateRequest updateRequest)
        {
            try
            {
                if (updateRequest.UserId == 0)
                {
                    return BadRequest(new CommonResponse { Id = 0, Message = "UserId is required for update", Status = false });
                }

                var request = new UserRequest
                {
                    Mode = updateRequest.Mode,
                    UserId = updateRequest.UserId,
                    Email = updateRequest.Email,
                    Password = updateRequest.Password,
                    CreatedBy = User?.Identity?.Name ?? "System"
                };

                var result = await _userBusiness.SaveUser(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userBusiness.LoginUser(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new CommonResponse { Id = 0, Message = ex.Message, Status = false });
            }
        }
    }
}
