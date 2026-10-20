using AutoMapper;
using MedLink.Application.DTOs.Common.identity;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Mapping
{
    public class UserMappingProfile :  Profile
    {
        public UserMappingProfile()
        {
            CreateMap<AppUser, RegisterDto>();
            CreateMap<RegisterDto, AppUser>();
            CreateMap<AppUser,LoginDto>();
            CreateMap<LoginDto , AppUser>();

            CreateMap<RegisterProviderRequest, AppUser>()
           .ForMember(dest => dest.PasswordHash,
            opt => opt.Ignore())
           .ForMember(dest => dest.IsActice,
            opt => opt.MapFrom(src => false));

            CreateMap<RegisterProviderRequest, ProviderProfile>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.userId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.user,
                    opt => opt.Ignore())
                .ForMember(dest => dest.LicenseFileId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ProviderLocations,
                    opt => opt.Ignore())
                .ForMember(dest => dest.verificationStatus,
                    opt => opt.MapFrom(
                        src => VerificationStatus.Pending));

            CreateMap<RegisterProviderRequest, ProviderLocations>()
                .ForMember(dest => dest.Id, 
                    opt => opt.Ignore())
                .ForMember(dest => dest.ProviderProfileId,
                    opt => opt.Ignore())
                .ForMember(dest => dest.ProviderProfile,
                    opt => opt.Ignore());

        }
    }
}
