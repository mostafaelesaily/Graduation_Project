using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using System;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    // Patient-facing booking actions: request a booking, view/manage own
    // bookings, and check slot availability before booking.
    public interface IPatientBookingService
    {
        Task<GetBookingDto> BookAsync(CreateBookingDto dto, string userId);
        Task<GetBookingDto> GetMyBookingByIdAsync(Guid bookingId, string userId);
        Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(string userId, int pageNumber, int pageSize);
        Task UpdateMyBookingNotesAsync(Guid bookingId, UpdateBookingDto dto, string userId);
        Task CancelMyBookingAsync(Guid bookingId, string userId);
        Task<bool> CheckAvailabilityAsync(Guid providerProfileId, DateTime bookingDate);
        Task<int> ExpireOverdueBookingsAsync(
            Guid? providerProfileId = null,
            DateTime? bookingDate = null,
            DateTime? asOfUtc = null);
    }
}
