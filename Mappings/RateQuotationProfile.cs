using AutoMapper;
using SSProjectSolution.Models;
using SSProjectSolution.Models.DTOs;

namespace SSProjectSolution.Mappings
{
    public class RateQuotationProfile : Profile
    {
        public RateQuotationProfile()
        {
            CreateMap<RateQuotationCreateDto, RateQuotationEntity>()
                .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId ?? 0))
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.CompanyName ?? string.Empty))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity ?? 0))
                .ForMember(d => d.CreatedBy, o => o.MapFrom(s => s.CreatedBy ?? 1));

            CreateMap<RateQuotationUpdateDto, RateQuotationEntity>()
                .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId ?? 0))
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.CompanyName ?? string.Empty))
                .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity ?? 0))
                .ForMember(d => d.ModifiedBy, o => o.MapFrom(s => s.ModifiedBy ?? 1));

            CreateMap<RateQuotationEntity, RateQuotationResponseDto>();
        }
    }
}
