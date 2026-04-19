using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public interface IUserService
    {
        Task<CommonResponse> ManageUserAsync(UserRequest request);
        Task<CommonResponse> LoginUserAsync(LoginRequest request);
    }
}
