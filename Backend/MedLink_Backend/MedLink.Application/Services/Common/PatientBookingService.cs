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
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MedLink.Domain.Entities.Provider;

namespace MedLink.Application.Services.Common
{
    // Patient-facing booking actions: request a booking, view/manage own
    // bookings, and check slot availability before booking. Provider
    // review/approval/cancellation actions live in ProviderBookingService.
    public class PatientBookingService : IPatientBookingService
    {
        private const int MaxNotesLength = 1000;
        private const int SqlServerUniqueViolation2601 = 2601;
        private const int SqlServerUniqueViolation2627 = 2627;

        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly UserManager<AppUser> userManager;
        private readonly ICacheService cacheService;
        private readonly ILogger<PatientBookingService> logger;

        public PatientBookingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            UserManager<AppUser> userManager,
            ICacheService cacheService,
            ILogger<PatientBookingService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.userManager = userManager;
            this.cacheService = cacheService;
            this.logger = logger;
        }

        public async Task<GetBookingDto> BookAsync(CreateBookingDto dto, string userId)
        {
            if (dto is null)
                throw new BadRequestException("Booking payload is required.");

            if (dto.ProviderProfileId == Guid.Empty)
                throw new BadRequestException("Provider profile id is required.");

            if (dto.BookingDate == default)
                throw new BadRequestException("Booking date is required.");

            if (dto.BookingDate <= DateTime.UtcNow)
                throw new BadRequestException("Booking date must be in the future.");

            var provider = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(dto.ProviderProfileId);
            if (provider is null)
                throw new NotFoundException("Provider profile not found.");

            if (provider.verificationStatus != VerificationStatus.Approved)
                throw new BadRequestException("Provider is not approved.");

            Booking? booking = null;

            await unitOfWork.BeginTransactionAsync();
            try
            {
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    await ExpireOverdueBookingsAsync(dto.ProviderProfileId, dto.BookingDate, DateTime.UtcNow);

                    // Determine whether an overdue Pending booking is still persisted.
                    // If expiration failed because another operation changed it, this
                    // lookup returns null and the subsequent conflict check determines
                    // the actual persisted status.
                    var overdueSpec = new OverduePendingBookingsSpecification(dto.ProviderProfileId, dto.BookingDate, DateTime.UtcNow);
                    var overdueBooking = await unitOfWork.Repository<Booking>().FindAsync(overdueSpec);
                    if (overdueBooking is not null)
                    {
                        if (attempt == 0)
                        {
                            continue;
                        }

                        throw new ConflictException("The requested booking slot is not available.");
                    }

                    if (!await IsWithinConfiguredAvailabilityAsync(dto.ProviderProfileId, dto.BookingDate))
                    {
                        throw new ConflictException("The requested booking time is outside provider availability.");
                    }

                    var conflictSpec = new BookingConflictSpecification(dto.ProviderProfileId, dto.BookingDate);
                    var existing = await unitOfWork.Repository<Booking>().FindAsync(conflictSpec);
                    if (existing is not null)
                        throw new ConflictException("The requested booking slot is not available.");

                    booking = new Booking
                    {
                        UserId = userId,
                        ProviderProfileId = dto.ProviderProfileId,
                        BookingDate = dto.BookingDate,
                        Status = BookingStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };

                    await unitOfWork.Repository<Booking>().AddAsync(booking);
                    await unitOfWork.SaveChangesAsync();
                    await unitOfWork.CommitTransactionAsync();

                    await InvalidateBookingCachesAsync(userId, dto.ProviderProfileId);

                    await SafeNotifyAsync(
                        userId,
                        new CreateNotificationDto
                        {
                            UserId = userId,
                            Title = "Booking Request Pending",
                            Message = "Your booking request is pending provider approval."
                        },
                        booking.Id);

                    if (!string.IsNullOrWhiteSpace(provider.userId))
                    {
                        await SafeNotifyAsync(
                            provider.userId,
                            new CreateNotificationDto
                            {
                                UserId = provider.userId,
                                Title = "New Booking Request",
                                Message = "A new booking request is pending your approval."
                            },
                            booking.Id);
                    }

                    return mapper.Map<GetBookingDto>(booking);
                }

                throw new ConflictException("The requested booking slot is not available.");
            }
            catch (DbUpdateException ex) when (IsBookingUniqueConstraintConflict(ex, booking))
            {
                await unitOfWork.RollbackTransactionAsync();
                throw new ConflictException("The requested booking slot is not available.");
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<GetBookingDto> GetMyBookingByIdAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            var isOwner = booking.UserId == userId;
            var isAdmin = await IsAdminAsync(userId);

            if (!isOwner && !isAdmin)
                throw new ForbiddenException("You are not allowed to access this booking.");

            return mapper.Map<GetBookingDto>(booking);
        }

        public async Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(string userId, int pageNumber, int pageSize)
        {
            var cacheKey = $"Get_MyBookings_userId:{userId}_pageNum:{pageNumber}_pageSize:{pageSize}";

            return await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var countSpec = new BookingsByUserSpecification(userId);
                var totalCount = await unitOfWork.Repository<Booking>().CountAsync(countSpec);

                var pagedSpec = new BookingsByUserSpecification(userId, pageNumber, pageSize);
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

        public async Task UpdateMyBookingNotesAsync(Guid bookingId, UpdateBookingDto dto, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);
            if (booking.UserId != userId && !await IsAdminAsync(userId))
                throw new ForbiddenException("You are not allowed to modify this booking.");

            if (dto != null && dto.Notes != null && dto.Notes.Length > MaxNotesLength)
                throw new BadRequestException("Notes cannot exceed 1000 characters.");

            // The current DTO cannot distinguish an omitted Notes value from an explicit null.
            // Preserve the existing value unless the client explicitly sends a replacement Notes value.
            if (dto != null && dto.Notes is not null)
                booking.Notes = dto.Notes;

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
        }

        public async Task CancelMyBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            var isOwner = booking.UserId == userId;
            var isAdmin = await IsAdminAsync(userId);

            if (!isOwner && !isAdmin)
                throw new ForbiddenException("You are not allowed to cancel this booking.");

            if (booking.Status == BookingStatus.Completed)
                throw new ConflictException("Completed bookings cannot be cancelled.");

            if (booking.Status == BookingStatus.Rejected)
                throw new ConflictException("Rejected bookings cannot be cancelled.");

            if (booking.Status == BookingStatus.Expired)
                throw new ConflictException("Expired bookings cannot be cancelled.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new ConflictException("Booking is already cancelled.");

            booking.Status = BookingStatus.Cancelled;
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

            if (!string.IsNullOrWhiteSpace(booking.UserId))
            {
                await SafeNotifyAsync(
                    booking.UserId,
                    new CreateNotificationDto
                    {
                        UserId = booking.UserId,
                        Title = "Booking Cancelled",
                        Message = "Your booking has been cancelled."
                    },
                    booking.Id);
            }

            if (booking.ProviderProfile is not null && !string.IsNullOrWhiteSpace(booking.ProviderProfile.userId))
            {
                await SafeNotifyAsync(
                    booking.ProviderProfile.userId,
                    new CreateNotificationDto
                    {
                        UserId = booking.ProviderProfile.userId,
                        Title = "Booking Cancelled",
                        Message = "A booking has been cancelled."
                    },
                    booking.Id);
            }
        }

        public async Task<bool> CheckAvailabilityAsync(Guid providerProfileId, DateTime bookingDate)
        {
            if (bookingDate <= DateTime.UtcNow)
                return false;

            var provider = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(providerProfileId);
            if (provider is null || provider.verificationStatus != VerificationStatus.Approved)
                return false;

            var spec = new BookingConflictSpecification(providerProfileId, bookingDate);
            var existing = await unitOfWork.Repository<Booking>().FindAsync(spec);
            if (existing is not null)
                return false;

            return await IsWithinConfiguredAvailabilityAsync(providerProfileId, bookingDate);
        }

        public async Task<int> ExpireOverdueBookingsAsync(
            Guid? providerProfileId = null,
            DateTime? bookingDate = null,
            DateTime? asOfUtc = null)
        {
            var now = asOfUtc ?? DateTime.UtcNow;
            var spec = new OverduePendingBookingsSpecification(providerProfileId, bookingDate, now);
            var overdue = await unitOfWork.Repository<Booking>().GetAllAsync(spec);

            if (overdue.Count == 0)
                return 0;

            int expiredCount = 0;

            foreach (var booking in overdue)
            {
                if (booking.Status != BookingStatus.Pending)
                    continue;

                if (booking.CreatedAt.AddHours(8) > now)
                    continue;

                var originalStatus = booking.Status;
                booking.Status = BookingStatus.Expired;
                unitOfWork.Repository<Booking>().Update(booking);

                try
                {
                    await unitOfWork.SaveChangesAsync();
                    expiredCount++;
                    await InvalidateBookingCachesAsync(booking.UserId, booking.ProviderProfileId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    booking.Status = originalStatus;
                }
                catch (DbUpdateException)
                {
                    booking.Status = originalStatus;
                }
            }

            return expiredCount;
        }

        private async Task<bool> IsWithinConfiguredAvailabilityAsync(Guid providerProfileId, DateTime bookingDate)
        {
            var availabilities = await unitOfWork.Repository<ProviderAvailability>().GetAllAsync(
                new ProviderAvailabilitiesByProviderSpecification(providerProfileId));
            if (availabilities.Count == 0)
                return true;

            var bookingTime = bookingDate.TimeOfDay;
            return availabilities.Any(a =>
                a.DayOfWeek == bookingDate.DayOfWeek &&
                a.StartTime <= bookingTime &&
                bookingTime < a.EndTime);
        }

        private async Task InvalidateBookingCachesAsync(string patientUserId, Guid providerProfileId)
        {
            await cacheService.RemoveAsync($"Get_MyBookings_userId:{patientUserId}");
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

        private async Task<bool> IsAdminAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null) return false;
            return await userManager.IsInRoleAsync(user, Roles.Admin);
        }

        private static bool IsBookingUniqueConstraintConflict(DbUpdateException exception, Booking? attemptedBooking)
        {
            if (attemptedBooking is null)
                return false;

            if (exception.Entries.Count == 0 ||
                !exception.Entries.Any(entry => entry.Entity is Booking))
            {
                return false;
            }

            var innerException = exception.InnerException;
            var message = innerException?.Message ?? exception.Message;

            var hasIndexEvidence = message.Contains("IX_Bookings_ProviderProfileId_BookingDate", StringComparison.OrdinalIgnoreCase)
                || (message.Contains("ProviderProfileId", StringComparison.OrdinalIgnoreCase)
                    && message.Contains("BookingDate", StringComparison.OrdinalIgnoreCase));

            if (!hasIndexEvidence)
                return false;

            if (innerException != null && IsSqlServerUniqueConstraintViolation(innerException))
                return true;

            return message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                || message.Contains("unique", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSqlServerUniqueConstraintViolation(Exception exception)
        {
            if (exception == null)
                return false;

            if (!exception.GetType().FullName?.Contains("SqlException", StringComparison.Ordinal) ?? true)
                return false;

            var numberProperty = exception.GetType().GetProperty("Number", BindingFlags.Public | BindingFlags.Instance);
            if (numberProperty?.GetValue(exception) is int number)
            {
                return number == SqlServerUniqueViolation2601
                    || number == SqlServerUniqueViolation2627;
            }

            return false;
        }
    }
}
