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

        public async Task<CommonResponse> SaveMeterInward(InwardMeterSaveRequest request)
        {
            foreach(var item in request.MeterDetails)
            {
                Console.WriteLine($"MeterValue : {item.MeterValue}");
            }

            // 1. Validate duplicates and empty rows
            var validMeterDetails = request.MeterDetails
                .Where(m => m.MeterValue > 0 && m.BitsCount > 0)
                .GroupBy(m => m.MeterValue)
                .Select(g => 
                {
                    var m = g.First();
                    // Recalculate TotalMeter
                    m.TotalMeter = m.MeterValue * m.BitsCount;
                    return m;
                })
                .ToList();

            if (!validMeterDetails.Any())
            {
                return new CommonResponse { Id = 0, Message = "No valid meter details provided. MeterValue and BitsCount must be greater than 0.", Status = false };
            }

            request.MeterDetails = validMeterDetails;

            // 2. Delegate to Service (recalculation happens in SP, but we cleaned up list here)
            var result = await _inwardService.SaveMeterInwardAsync(request);

            if (result.InwardId > 0)
            {
                return new CommonResponse { Id = result.InwardId, Message = result.Message, Status = true };
            }

            return new CommonResponse { Id = 0, Message = result.Message, Status = false };
        }
    }
}
