using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Domain.Entities.Provider;

namespace MedLink.Application.Mapping
{
    public class ProviderMappingProfile : Profile
    {
        public ProviderMappingProfile()
        {
            CreateMap<ProviderProfile, GetProviderRequestDto>()
                .ForMember(dest => dest.ProviderId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => string.Join(", ", src.ProviderServices.Select(service => service.ServiceName))))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.user.PhoneNumber))
                .ForMember(dest => dest.LicenseImageUrl,
                    opt => opt.Ignore());
        }
    }
}
