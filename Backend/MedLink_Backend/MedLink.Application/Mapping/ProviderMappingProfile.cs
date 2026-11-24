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
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.user != null ? src.user.UserName : null))
                .ForMember(dest => dest.Email,
                    opt => opt.MapFrom(src => src.user != null ? src.user.Email : null))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.user != null ? src.user.PhoneNumber : null))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.verificationStatus))
                .ForMember(dest => dest.LicenseImageUrl,
                    opt => opt.Ignore());

            CreateMap<ProviderAvailability, GetAvailabilityDto>();
            CreateMap<GetAvailabilityDto, ProviderAvailability>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderProfileId, opt => opt.Ignore())
                .ForMember(dest => dest.ProviderProfile, opt => opt.Ignore());
        }
    }
}
