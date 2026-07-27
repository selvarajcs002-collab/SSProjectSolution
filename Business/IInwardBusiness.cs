using SSProjectSolution.Request;
using SSProjectSolution.Response;

namespace SSProjectSolution.Business
{
    public interface IInwardBusiness
    {
        Task<CommonResponse> SaveInward(InwardSaveRequest request);
        Task<CommonResponse> SaveMeterInward(InwardMeterSaveRequest request);
        Task<CommonResponse> SaveMultipleColourInward(InwardMultipleColourSaveRequest request);
    }
}
