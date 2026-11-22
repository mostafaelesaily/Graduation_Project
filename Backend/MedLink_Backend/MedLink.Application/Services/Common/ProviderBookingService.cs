using AutoMapper;
using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.Bookings;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Services.Common
{
    // Provider-management booking actions: review and approve/reject
    // bookings made against the current provider's profile.
    // Cancelling a booking is a patient-only action; patient-side actions
    // (creating/cancelling a patient's own booking) live in PatientBookingService.
    public class ProviderBookingService : IProviderBookingService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly UserManager<AppUser> userManager;
        private readonly ICacheService cacheService;
        private readonly ILogger<ProviderBookingService> logger;

        public ProviderBookingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            UserManager<AppUser> userManager,
            ICacheService cacheService,
            ILogger<ProviderBookingService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.userManager = userManager;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task<GetBookingDto> GetBookingByIdAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (!await IsProviderOwnerAsync(booking, userId) && !await IsAdminAsync(userId))
                throw new ForbiddenException("You are not allowed to access this booking.");

            return mapper.Map<GetBookingDto>(booking);
        }

        public async Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
        {
            var profile = await unitOfWork.Repository<ProviderProfile>().FindAsync(new ProviderProfileByUserSpecification(userId));
            if (profile is null)
                throw new NotFoundException("Provider profile not found.");

            return await GetBookingsByProviderIdAsync(profile.Id, userId, pageNumber, pageSize, filter, startDate, endDate, search);
        }

        public async Task<PaginatedResultDto<GetBookingDto>> GetBookingsByProviderIdAsync(
            Guid providerProfileId,
            string userId,
            int pageNumber,
            int pageSize,
            string? filter = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? search = null)
        {
            var provider = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(providerProfileId);
            if (provider is null)
                throw new NotFoundException("Provider profile not found.");

            if (provider.userId != userId && !await IsAdminAsync(userId))
                throw new ForbiddenException("You are not allowed to access these bookings.");

            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
                throw new BadRequestException("Start date cannot be later than end date.");

            var cacheKey = $"Get_ProviderBookings_providerProfileId:{providerProfileId}_pageNum:{pageNumber}_pageSize:{pageSize}_filter:{filter}_startDate:{startDate}_endDate:{endDate}_search:{search}";

            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var countSpec = new BookingsByProviderSpecification(providerProfileId, filter, startDate, endDate, search);
                var totalCount = await unitOfWork.Repository<Booking>().CountAsync(countSpec);

                var pagedSpec = new BookingsByProviderSpecification(providerProfileId, pageNumber, pageSize, filter, startDate, endDate, search);
                var list = await unitOfWork.Repository<Booking>().GetAllAsync(pagedSpec);

                return new PaginatedResultDto<GetBookingDto>
                {
                    Data = mapper.Map<IReadOnlyList<GetBookingDto>>(list),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            });
        }

        public async Task<ProviderBookingStatisticsDto> GetBookingStatisticsAsync(string userId)
        {
            var profile = await unitOfWork.Repository<ProviderProfile>().FindAsync(new ProviderProfileByUserSpecification(userId));
            if (profile is null)
                throw new NotFoundException("Provider profile not found.");

            var cacheKey = $"Get_ProviderBookingStatistics_providerProfileId:{profile.Id}";

            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var now = DateTime.UtcNow;
                var todayStart = now.Date;
                var sevenDaysEnd = todayStart.AddDays(7);

                var todayCount = await unitOfWork.Repository<Booking>().CountAsync(
                    new ProviderTodayBookingsSpecification(profile.Id, todayStart));
                var nextSevenDaysCount = await unitOfWork.Repository<Booking>().CountAsync(
                    new ProviderNextSevenDaysBookingsSpecification(profile.Id, now, sevenDaysEnd));
                var pendingCount = await unitOfWork.Repository<Booking>().CountAsync(
                    new ProviderPendingBookingsSpecification(profile.Id));
                var cancelledCount = await unitOfWork.Repository<Booking>().CountAsync(
                    new ProviderCancelledBookingsSpecification(profile.Id));

                return new ProviderBookingStatisticsDto
                {
                    TodayCount = todayCount,
                    NextSevenDaysCount = nextSevenDaysCount,
                    PendingCount = pendingCount,
                    CancelledCount = cancelledCount
                };
            });
        }

        public async Task<GetBookingDto> ApproveBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (!await IsProviderOwnerAsync(booking, userId))
                throw new ForbiddenException("You are not allowed to approve this booking.");

            if (booking.Status != BookingStatus.Pending)
                throw new ConflictException("Only pending bookings can be approved.");

            if (booking.CreatedAt.AddHours(8) <= DateTime.UtcNow)
            {
                booking.Status = BookingStatus.Expired;
                unitOfWork.Repository<Booking>().Update(booking);
                try
                {
                    await unitOfWork.SaveChangesAsync();
                    await InvalidateBookingCachesAsync(booking.UserId, booking.ProviderProfileId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Ignore concurrency failures while recording expiration.
                }

                throw new ConflictException("Booking has expired.");
            }

            booking.Status = BookingStatus.Confirmed;
            unitOfWork.Repository<Booking>().Update(booking);
            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The booking was changed by another request.");
            }

            await InvalidateBookingCachesAsync(booking.UserId, booking.ProviderProfileId);

            await SafeNotifyAsync(
                booking.UserId,
                new CreateNotificationDto
                {
                    UserId = booking.UserId,
                    Title = "Booking Confirmed",
                    Message = "Your booking has been confirmed."
                },
                booking.Id);

            return mapper.Map<GetBookingDto>(booking);
        }

        public async Task RejectBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (!await IsProviderOwnerAsync(booking, userId))
                throw new ForbiddenException("You are not allowed to reject this booking.");

            if (booking.Status != BookingStatus.Pending)
                throw new ConflictException("Only pending bookings can be rejected.");

            if (booking.CreatedAt.AddHours(8) <= DateTime.UtcNow)
            {
                booking.Status = BookingStatus.Expired;
                unitOfWork.Repository<Booking>().Update(booking);
                try
                {
                    await unitOfWork.SaveChangesAsync();
                    await InvalidateBookingCachesAsync(booking.UserId, booking.ProviderProfileId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Ignore concurrency failures while recording expiration.
                }

                throw new ConflictException("Booking has expired.");
            }

            booking.Status = BookingStatus.Rejected;
            unitOfWork.Repository<Booking>().Update(booking);
            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException("The booking was changed by another request.");
            }

            await InvalidateBookingCachesAsync(booking.UserId, booking.ProviderProfileId);

            await SafeNotifyAsync(
                booking.UserId,
                new CreateNotificationDto
                {
                    UserId = booking.UserId,
                    Title = "Booking Rejected",
                    Message = "Your booking request has been rejected."
                },
                booking.Id);
        }

        private async Task InvalidateBookingCachesAsync(string patientUserId, Guid providerProfileId)
        {
            await cacheService.RemoveAsync($"Get_MyBookings_userId:{patientUserId}");
            await InvalidateBookingCachesAsync(providerProfileId);
        }

        private async Task InvalidateBookingCachesAsync(Guid providerProfileId)
        {
            await cacheService.RemoveAsync($"Get_ProviderBookings_providerProfileId:{providerProfileId}");
            await cacheService.RemoveAsync($"Get_ProviderBookingStatistics_providerProfileId:{providerProfileId}");
        }

        private async Task SafeNotifyAsync(string recipientUserId, CreateNotificationDto notification, Guid bookingId)
        {
            try
            {
                await notificationService.SendToUserAsync(recipientUserId, notification);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification to {RecipientUserId} for booking {BookingId}.", recipientUserId, bookingId);
            }
        }

        private async Task<Booking> LoadBookingAsync(Guid bookingId)
        {
            var spec = new BookingByIdSpecification(bookingId);
            var booking = await unitOfWork.Repository<Booking>().FindAsync(spec);
            if (booking is null)
                throw new NotFoundException("Booking not found.");
            return booking;
        }

        private async Task<bool> IsProviderOwnerAsync(Booking booking, string userId)
        {
            if (booking.ProviderProfile is not null && booking.ProviderProfile.userId == userId)
                return true;

            var profile = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(booking.ProviderProfileId);
            return profile is not null && profile.userId == userId;
        }

        private async Task<bool> IsAdminAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return false;
            return await userManager.IsInRoleAsync(user, Roles.Admin);
        }
    }
}
