using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Business
{
    public interface IUserBusiness
    {
        Task<CommonResponse> SaveUser(UserRequest request);
        Task<CommonResponse> LoginUser(LoginRequest request);
    }
}
