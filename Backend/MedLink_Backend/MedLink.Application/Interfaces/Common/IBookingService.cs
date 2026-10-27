using MedLink.Application.DTOs.Common.BookingDTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto, string userId);
        Task<BookingDto> ApproveBookingAsync(Guid bookingId, string userId);
        Task<BookingDto> RejectBookingAsync(Guid bookingId, string userId);
        Task<BookingDto> UpdateNotesAsync(Guid bookingId, UpdateBookingDto dto, string userId);
        Task<BookingDto> GetBookingAsync(Guid bookingId, string userId);
        Task<IReadOnlyList<BookingDto>> GetMyBookingsAsync(string userId);
        Task<IReadOnlyList<BookingDto>> GetProviderBookingsAsync(Guid providerProfileId, string userId);
        Task CancelBookingAsync(Guid bookingId, string userId);
        Task<bool> CheckAvailabilityAsync(Guid providerProfileId, DateTime bookingDate);
    }
}
