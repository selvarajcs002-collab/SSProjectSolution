using SSProjectSolution.Request;
using SSProjectSolution.Response;
using SSProjectSolution.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System;
namespace SSProjectSolution.Business
{
    public class InwardBusiness : IInwardBusiness
    {
        private readonly IInwardService _inwardService;
        private readonly ILogger<InwardBusiness> _logger;

        public InwardBusiness(IInwardService inwardService, ILogger<InwardBusiness> logger)
        {
            _inwardService = inwardService;
            _logger = logger;
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

        public async Task<CommonResponse> SaveMultipleColourInward(InwardMultipleColourSaveRequest request)
        {
            try
            {
                if (request == null || request.Inward == null || request.Colours == null || request.Sizes == null || request.Matrix == null)
                {
                    return new CommonResponse { Id = 0, Message = "Invalid request payload", Status = false };
                }

                var validColours = request.Colours.Select(c => c?.Trim().ToUpper()).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToHashSet();
                var validSizes = request.Sizes.Select(s => s?.Trim().ToUpper()).Where(s => !string.IsNullOrEmpty(s)).Distinct().ToHashSet();

                if (!validColours.Any() || !validSizes.Any())
                {
                    return new CommonResponse { Id = 0, Message = "Colours and Sizes cannot be empty", Status = false };
                }

                var matrixGroups = request.Matrix
                    .Where(m => m.Count.HasValue && m.Count.Value > 0)
                    .Select(m => new { Colour = m.Colour?.Trim().ToUpper(), Size = m.Size?.Trim().ToUpper(), Count = m.Count.Value })
                    .Where(m => m.Colour != null && validColours.Contains(m.Colour) && m.Size != null && validSizes.Contains(m.Size))
                    .GroupBy(m => m.Colour!)
                    .ToDictionary(g => g.Key, g => g.ToList());

                if (!matrixGroups.Any())
                {
                    return new CommonResponse { Id = 0, Message = "Matrix must contain valid entries with count > 0", Status = false };
                }

                using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var colourGroup in matrixGroups)
                    {
                        var colour = colourGroup.Key;
                        var sizes = colourGroup.Value
                            .GroupBy(m => m.Size!)
                            .Select(g => new SizeDto { Size = g.Key, Count = g.Sum(m => m.Count) })
                            .ToList();

                        var oldRequest = new InwardSaveRequest
                        {
                            Inward = new InwardCreateDto
                            {
                                CompanyId = request.Inward.CompanyId,
                                Colour = colour,
                                DesignName = request.Inward.DesignName,
                                StyleNo = request.Inward.StyleNo,
                                InwardDcNo = request.Inward.InwardDcNo,
                                PoNo = request.Inward.PoNo,
                                UploadURL = request.Inward.UploadURL,
                                CreatedBy = request.Inward.CreatedBy,
                                InwardDate = request.Inward.InwardDate
                            },
                            Sizes = sizes
                        };

                        var saveResult = await this.SaveInward(oldRequest);
                        if (!saveResult.Status)
                        {
                            _logger.LogError($"Failed to save colour {colour}: {saveResult.Message}");
                            return new CommonResponse { Id = 0, Message = $"Failed to save colour {colour}", Status = false };
                        }
                    }

                    scope.Complete();
                    return new CommonResponse { Id = 1, Message = "Inward Saved Successfully", Status = true };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during SaveMultipleColourInward");
                return new CommonResponse { Id = 0, Message = "An unexpected error occurred while saving", Status = false };
            }
        }
    }
}
