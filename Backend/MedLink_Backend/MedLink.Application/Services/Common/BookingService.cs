using AutoMapper;
using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.Bookings;
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

namespace MedLink.Application.Services.Common
{
    public class BookingService : IBookingService
    {
        private const int MaxNotesLength = 1000;
        private const int SqlServerUniqueViolation2601 = 2601;
        private const int SqlServerUniqueViolation2627 = 2627;

        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly UserManager<AppUser> userManager;
        private readonly ILogger<BookingService> logger;

        public BookingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            UserManager<AppUser> userManager,
            ILogger<BookingService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<GetBookingDto> CreateBookingAsync(CreateBookingDto dto, string userId)
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

            var conflictSpec = new BookingConflictSpecification(dto.ProviderProfileId, dto.BookingDate);
            var existing = await unitOfWork.Repository<Booking>().FindAsync(conflictSpec);
            if (existing is not null)
                throw new ConflictException("The requested booking slot is not available.");

            var booking = new Booking
            {
                UserId = userId,
                ProviderProfileId = dto.ProviderProfileId,
                BookingDate = dto.BookingDate,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.BeginTransactionAsync();
            try
            {
                await unitOfWork.Repository<Booking>().AddAsync(booking);
                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
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

        public async Task<GetBookingDto> ApproveBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            if (!await IsProviderOwnerAsync(booking, userId))
                throw new ForbiddenException("You are not allowed to approve this booking.");

            if (booking.Status != BookingStatus.Pending)
                throw new ConflictException("Only pending bookings can be approved.");

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

        public async Task UpdateNotesAsync(Guid bookingId, UpdateBookingDto dto, string userId)
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
        }

        public async Task<GetBookingDto> GetBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            var isPatient = booking.UserId == userId;
            var isProviderOwner = await IsProviderOwnerAsync(booking, userId);
            var isAdmin = await IsAdminAsync(userId);

            if (!isPatient && !isProviderOwner && !isAdmin)
                throw new ForbiddenException("You are not allowed to access this booking.");

            return mapper.Map<GetBookingDto>(booking);
        }

        public async Task<PaginatedResultDto<GetBookingDto>> GetMyBookingsAsync(string userId, int pageNumber, int pageSize)
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
        }

        public async Task<PaginatedResultDto<GetBookingDto>> GetProviderBookingsAsync(Guid providerProfileId, string userId, int pageNumber, int pageSize)
        {
            var provider = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(providerProfileId);
            if (provider is null)
                throw new NotFoundException("Provider profile not found.");

            if (provider.userId != userId && !await IsAdminAsync(userId))
                throw new ForbiddenException("You are not allowed to access these bookings.");

            var countSpec = new BookingsByProviderSpecification(providerProfileId);
            var totalCount = await unitOfWork.Repository<Booking>().CountAsync(countSpec);

            var pagedSpec = new BookingsByProviderSpecification(providerProfileId, pageNumber, pageSize);
            var list = await unitOfWork.Repository<Booking>().GetAllAsync(pagedSpec);

            return new PaginatedResultDto<GetBookingDto>
            {
                Data = mapper.Map<IReadOnlyList<GetBookingDto>>(list),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task CancelBookingAsync(Guid bookingId, string userId)
        {
            var booking = await LoadBookingAsync(bookingId);

            var isPatient = booking.UserId == userId;
            var isProviderOwner = await IsProviderOwnerAsync(booking, userId);
            var isAdmin = await IsAdminAsync(userId);

            if (!isPatient && !isProviderOwner && !isAdmin)
                throw new ForbiddenException("You are not allowed to cancel this booking.");

            if (booking.Status == BookingStatus.Completed)
                throw new ConflictException("Completed bookings cannot be cancelled.");

            if (booking.Status == BookingStatus.Rejected)
                throw new ConflictException("Rejected bookings cannot be cancelled.");

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
            return existing is null;
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

        private static bool IsBookingUniqueConstraintConflict(DbUpdateException exception, Booking attemptedBooking)
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
