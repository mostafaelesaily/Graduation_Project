using AutoMapper;
using MedLink.Application.DTOs.Common.MedicalSearchDTOs;
using MedLink.Domain.Entities.Provider;

namespace MedLink.Application.Mapping
{
    public class MedicalSearchMappingProfile : Profile
    {
        public MedicalSearchMappingProfile()
        {
            // Map ProviderProfile to GetProviderResultDto (for unified search)
            CreateMap<ProviderProfile, GetProviderResultDto>()
                .ForMember(dest => dest.ProviderId,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => string.Join(", ", src.ProviderServices
                        .Where(s => s.IsActive)
                        .Select(s => s.ServiceName))))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.ProviderLocations
                        .Where(l => l.IsActive)
                        .Select(l => l.Address)
                        .FirstOrDefault()))
                .ForMember(dest => dest.PhoneNumber,
                    opt => opt.MapFrom(src => src.user.PhoneNumber));
        }
    }
}
