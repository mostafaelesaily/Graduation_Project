using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;

namespace MedLink.Application.Mapping
{
    public class ProviderManagementMappingProfile : Profile
    {
        public ProviderManagementMappingProfile()
        {
            CreateMap<ProviderProfile, GetProviderProfileDto>()
                .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProviderType, opt => opt.MapFrom(src => src.ProviderType.ToString()))
                .ForMember(dest => dest.VerificationStatus, opt => opt.MapFrom(src => src.verificationStatus.ToString()))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.user.IsActice))
                .ForMember(dest => dest.HasAvailabilityConfigured, opt => opt.MapFrom(src => src.HasAvailabilityConfigured));

            CreateMap<ProviderLocations, GetProviderLocationDto>();

            CreateMap<CreateProviderLocationDto, ProviderLocations>();

            CreateMap<UpdateProviderLocationDto, ProviderLocations>();

            CreateMap<StoredFile, GetProviderDocumentDto>();

            CreateMap<ProviderService, GetProviderServiceDto>();

            CreateMap<CreateProviderServiceDto, ProviderService>();

            CreateMap<UpdateProviderServiceDto, ProviderService>();

            CreateMap<MedicalRecord, MedicalRecordDto>();

            CreateMap<CreateMedicalRecordDto, MedicalRecord>();

            CreateMap<Review, ReviewDto>();

            CreateMap<CreateReviewDto, Review>();

            CreateMap<ProviderAvailability, ProviderAvailabilityDto>();

            CreateMap<ProviderAvailabilityDto, ProviderAvailability>();
        }
    }
}