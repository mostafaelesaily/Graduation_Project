using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Domain.Entities.Common;

namespace MedLink.Application.Mapping
{
    public class ProviderMappingProfile : Profile
    {
        public ProviderMappingProfile()
        {
            CreateMap<ProviderProfile, GetProviderRequestDto>()
                .ForMember(dest => dest.ProviderId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.user.PhoneNumber))
                .ForMember(dest => dest.LicenseImageUrl,
                    opt => opt.Ignore());
        }
    }
}
