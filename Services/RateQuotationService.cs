using AutoMapper;
using Microsoft.Extensions.Logging;
using SSProjectSolution.Models;
using SSProjectSolution.Models.DTOs;
using SSProjectSolution.Repositories;
using SSProjectSolution.Response;

namespace SSProjectSolution.Services
{
    public class RateQuotationService : IRateQuotationService
    {
        private readonly IRateQuotationRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<RateQuotationService> _logger;

        public RateQuotationService(IRateQuotationRepository repository, IMapper mapper, ILogger<RateQuotationService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<long>> CreateAsync(RateQuotationCreateDto createDto)
        {
            try
            {
                var entity = _mapper.Map<RateQuotationEntity>(createDto);
                var (newId, statusCode, statusMessage) = await _repository.CreateAsync(entity);

                if (statusCode == 201)
                {
                    return ApiResponse<long>.SuccessResponse(newId, statusMessage);
                }
                
                _logger.LogWarning("Failed to create RateQuotation: {Message} - Status: {Code}", statusMessage, statusCode);
                return ApiResponse<long>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating RateQuotation.");
                return ApiResponse<long>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(long id, RateQuotationUpdateDto updateDto)
        {
            try
            {
                var entity = _mapper.Map<RateQuotationEntity>(updateDto);
                entity.Id = id;
                var (statusCode, statusMessage) = await _repository.UpdateAsync(entity);

                if (statusCode == 200)
                {
                    return ApiResponse<bool>.SuccessResponse(true, statusMessage);
                }

                _logger.LogWarning("Failed to update RateQuotation: {Message} - Status: {Code}", statusMessage, statusCode);
                return ApiResponse<bool>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while updating RateQuotation for Id {Id}.", id);
                return ApiResponse<bool>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(long id, long modifiedBy)
        {
            try
            {
                var (statusCode, statusMessage) = await _repository.DeleteAsync(id, modifiedBy);

                if (statusCode == 200)
                {
                    return ApiResponse<bool>.SuccessResponse(true, statusMessage);
                }

                _logger.LogWarning("Failed to delete RateQuotation: {Message} - Status: {Code}", statusMessage, statusCode);
                return ApiResponse<bool>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while deleting RateQuotation for Id {Id}.", id);
                return ApiResponse<bool>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        private readonly string ImageFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "RateQuotationImages");

        private void AttachImageInfo(RateQuotationResponseDto dto)
        {
            if (dto == null) return;
            try
            {
                if (Directory.Exists(ImageFolderPath))
                {
                    var files = Directory.GetFiles(ImageFolderPath, $"{dto.Id}.*");
                    if (files.Length > 0)
                    {
                        dto.HasImage = true;
                        dto.ImageUrl = $"/api/RateQuotation/image/{dto.Id}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking image existence for RateQuotation Id {Id}", dto.Id);
            }
        }

        public async Task<ApiResponse<RateQuotationResponseDto>> GetByIdAsync(long id)
        {
            try
            {
                var (entity, statusCode, statusMessage) = await _repository.GetByIdAsync(id);

                if (statusCode == 200 && entity != null)
                {
                    var dto = _mapper.Map<RateQuotationResponseDto>(entity);
                    AttachImageInfo(dto);
                    return ApiResponse<RateQuotationResponseDto>.SuccessResponse(dto, statusMessage);
                }

                _logger.LogWarning("RateQuotation not found: {Message} - Status: {Code}", statusMessage, statusCode);
                return ApiResponse<RateQuotationResponseDto>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching RateQuotation for Id {Id}.", id);
                return ApiResponse<RateQuotationResponseDto>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        public async Task<ApiResponse<IEnumerable<RateQuotationResponseDto>>> GetAllAsync()
        {
            try
            {
                var (entities, statusCode, statusMessage) = await _repository.GetAllAsync();

                if (statusCode == 200)
                {
                    var dtos = _mapper.Map<IEnumerable<RateQuotationResponseDto>>(entities);
                    foreach (var dto in dtos)
                    {
                        AttachImageInfo(dto);
                    }
                    return ApiResponse<IEnumerable<RateQuotationResponseDto>>.SuccessResponse(dtos, statusMessage);
                }

                return ApiResponse<IEnumerable<RateQuotationResponseDto>>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching all RateQuotations.");
                return ApiResponse<IEnumerable<RateQuotationResponseDto>>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        public async Task<ApiResponse<IEnumerable<RateQuotationResponseDto>>> SearchAsync(RateQuotationSearchDto searchDto)
        {
            try
            {
                var (entities, statusCode, statusMessage) = await _repository.SearchAsync(searchDto);

                if (statusCode == 200)
                {
                    var dtos = _mapper.Map<IEnumerable<RateQuotationResponseDto>>(entities);
                    foreach (var dto in dtos)
                    {
                        AttachImageInfo(dto);
                    }
                    return ApiResponse<IEnumerable<RateQuotationResponseDto>>.SuccessResponse(dtos, statusMessage);
                }

                return ApiResponse<IEnumerable<RateQuotationResponseDto>>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while searching RateQuotations.");
                return ApiResponse<IEnumerable<RateQuotationResponseDto>>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }

        public async Task<ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>> GetPagedAsync(RateQuotationSearchDto searchDto)
        {
            try
            {
                var (entities, totalRecords, statusCode, statusMessage) = await _repository.GetPagedAsync(searchDto);

                if (statusCode == 200)
                {
                    var dtos = _mapper.Map<IEnumerable<RateQuotationResponseDto>>(entities);
                    foreach (var dto in dtos)
                    {
                        AttachImageInfo(dto);
                    }
                    var pagedResponse = new PagedResponse<IEnumerable<RateQuotationResponseDto>>(dtos, searchDto.PageNumber, searchDto.PageSize, totalRecords);
                    return ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>.SuccessResponse(pagedResponse, statusMessage);
                }

                return ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>.ErrorResponse(statusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching paged RateQuotations.");
                return ApiResponse<PagedResponse<IEnumerable<RateQuotationResponseDto>>>.ErrorResponse("An internal error occurred. Please try again later.");
            }
        }
    }
}
