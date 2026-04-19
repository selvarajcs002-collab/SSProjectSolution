using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Services;

namespace SSProjectSolution.Business
{
    public class InwardBusiness : IInwardBusiness
    {
        private readonly IInwardService _inwardService;

        public InwardBusiness(IInwardService inwardService)
        {
            _inwardService = inwardService;
        }

        public async Task<CommonResponse> SaveInward(InwardSaveRequest request)
        {
            // 1. Trim inputs
            request.Inward.Colour = request.Inward.Colour?.Trim() ?? string.Empty;
            request.Inward.DesignName = request.Inward.DesignName?.Trim() ?? string.Empty;
            request.Inward.StyleNo = request.Inward.StyleNo?.Trim() ?? string.Empty;

            // 2. Insert Header
            int inwardId = await _inwardService.SaveInwardAsync(request.Inward);

            if (inwardId > 0)
            {
                // 3. Insert Sizes (mapping redundant parent fields)
                if (request.Sizes != null && request.Sizes.Count > 0)
                {
                    await _inwardService.SaveInwardSizeCountsAsync(
                        inwardId, 
                        request.Inward.StyleNo, 
                        request.Inward.DesignName, 
                        request.Inward.Colour, 
                        request.Sizes);
                }

                return new CommonResponse { Id = inwardId, Message = "Inward Saved Successfully", Status = true };
            }

            return new CommonResponse { Id = 0, Message = "Failed to save Inward", Status = false };
        }
    }
}
