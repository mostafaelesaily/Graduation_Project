using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using System;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    // Provider-management booking actions: review, approve/reject and
    // inspect bookings made against the current provider's profile.
    // Cancelling a booking is a patient-only action (see IPatientBookingService).
    public interface IProviderBookingService
    {
        Task<GetBookingDto> GetBookingByIdAsync(Guid bookingId, string userId);
        Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);
        Task<PaginatedResultDto<GetBookingDto>> GetBookingsByProviderIdAsync(
            Guid providerProfileId,
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);
        Task<ProviderBookingStatisticsDto> GetBookingStatisticsAsync(string userId);
        Task<GetBookingDto> ApproveBookingAsync(Guid bookingId, string userId);
        Task RejectBookingAsync(Guid bookingId, string userId);
    }
}
