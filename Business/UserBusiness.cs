using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Services;

namespace SSProjectSolution.Business
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IUserService _userService;

        public UserBusiness(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<CommonResponse> SaveUser(UserRequest request)
        {
            // Business logic can be added here (e.g. password hashing, validation)
            return await _userService.ManageUserAsync(request);
        }

        public async Task<CommonResponse> LoginUser(LoginRequest request)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return new CommonResponse
                {
                    Id = 0,
                    Message = "Email and Password are required",
                    Status = false
                };
            }

            return await _userService.LoginUserAsync(request);
        }
    }
}
