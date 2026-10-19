using AutoMapper;
using MedLink.Application.DTOs.Common.identity;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
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
        }
    }
}
