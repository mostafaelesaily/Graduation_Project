using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Domain.Entities.Common;
using System;

namespace MedLink.Application.Mapping
{
    // Shared read mapping: used by both the patient-facing and the
    // provider-management booking services to project a Booking entity.
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<Booking, GetBookingDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null));
        }
    }
}
