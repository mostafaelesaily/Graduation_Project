using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Domain.Entities.Common;
using System;

namespace MedLink.Application.Mapping
{
    public class BookingMappingProfile : Profile
    {
        public BookingMappingProfile()
        {
            CreateMap<Booking, GetBookingDto>()
                .ForMember(dest => dest.PatientName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.FullName : null));
            CreateMap<CreateBookingDto, Booking>();
            CreateMap<UpdateBookingDto, Booking>()
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));
        }
    }
}
