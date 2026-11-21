using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using System;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface IBookingService
    {
        Task<GetBookingDto> CreateBookingAsync(CreateBookingDto dto, string userId);
        Task<GetBookingDto> ApproveBookingAsync(Guid bookingId, string userId);
        Task RejectBookingAsync(Guid bookingId, string userId);
        Task UpdateNotesAsync(Guid bookingId, UpdateBookingDto dto, string userId);
        Task<GetBookingDto> GetBookingAsync(Guid bookingId, string userId);
        Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(string userId, int pageNumber, int pageSize);
        Task<PaginatedResultDto<GetBookingDto>> GetProviderBookingsAsync(
            Guid providerProfileId,
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);
        Task<PaginatedResultDto<GetBookingDto>> GetMyProviderBookingsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null);
        Task<ProviderBookingStatisticsDto> GetProviderBookingStatisticsAsync(string userId);
        Task CancelBookingAsync(Guid bookingId, string userId);
        Task<bool> CheckAvailabilityAsync(Guid providerProfileId, DateTime bookingDate);
        Task<int> ExpireOverdueBookingsAsync(
            Guid? providerProfileId = null,
            DateTime? bookingDate = null,
            DateTime? asOfUtc = null);
    }
}
