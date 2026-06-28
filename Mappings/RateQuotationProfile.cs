using AutoMapper;
using SSProjectSolution.Models;
using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Mappings
{
    public class RateQuotationProfile : Profile
    {
        public RateQuotationProfile()
        {
            CreateMap<RateQuotationCreateDto, RateQuotationEntity>();
            CreateMap<RateQuotationUpdateDto, RateQuotationEntity>();
            CreateMap<RateQuotationEntity, RateQuotationResponseDto>();
        }
    }
}
