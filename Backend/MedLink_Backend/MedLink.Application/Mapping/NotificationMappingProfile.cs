using AutoMapper;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Mapping
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            CreateMap<Notifications, CreateNotificationDto>();
            CreateMap<CreateNotificationDto, Notifications>();
            CreateMap<GetNotificationDto, Notifications>();
            CreateMap<Notifications, GetNotificationDto>();
        }
    }
}
