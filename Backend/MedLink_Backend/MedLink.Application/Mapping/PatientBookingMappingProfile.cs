using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Domain.Entities.Common;

namespace MedLink.Application.Mapping
{
    // Patient-originated booking write mappings: creating a booking and
    // editing the notes on an existing one.
    public class PatientBookingMappingProfile : Profile
    {
        public PatientBookingMappingProfile()
        {
            CreateMap<CreateBookingDto, Booking>();
            CreateMap<UpdateBookingDto, Booking>()
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));
        }
    }
}
