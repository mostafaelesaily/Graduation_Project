using System;
using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Application.Specifications.Bookings;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedLink.Test.Services
{
    public class BookingServiceTest
    {
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<ILogger<BookingService>> loggerMock = new();
        private readonly Mock<UserManager<AppUser>> userManagerMock;
        private readonly Mock<INotificationService> notificationServiceMock = new();
        private readonly Mock<IMailService> mailServiceMock = new();
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IGenaricRepo<Booking>> bookingRepositoryMock = new();
        private readonly Mock<IGenaricRepo<ProviderProfile>> providerRepositoryMock = new();
        private readonly Mock<IGenaricRepo<ProviderAvailability>> availabilityRepositoryMock = new();

        private readonly BookingService sut;

        private static readonly Guid ProviderId = Guid.NewGuid();
        private const string ProviderUserId = "provider-user";
        private const string PatientId = "patient-user";

        public BookingServiceTest()
        {
            var storeMock = new Mock<IUserStore<AppUser>>();
            userManagerMock = new Mock<UserManager<AppUser>>(
                storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            unitOfWorkMock.Setup(u => u.Repository<Booking>()).Returns(bookingRepositoryMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderProfile>()).Returns(providerRepositoryMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderAvailability>()).Returns(availabilityRepositoryMock.Object);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>());
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new List<Booking>());
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>())).ReturnsAsync(0);
            unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            mapperMock.Setup(m => m.Map<GetBookingDto>(It.IsAny<Booking>()))
                .Returns((Booking booking) => new GetBookingDto
                {
                    Id = booking.Id,
                    UserId = booking.UserId,
                    ProviderProfileId = booking.ProviderProfileId,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    Notes = booking.Notes,
                    CreatedAt = booking.CreatedAt
                });

            mapperMock.Setup(m => m.Map<IReadOnlyList<GetBookingDto>>(It.IsAny<IEnumerable<Booking>>()))
                .Returns((IEnumerable<Booking> bookings) => bookings.Select(b => new GetBookingDto
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    ProviderProfileId = b.ProviderProfileId,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    Notes = b.Notes,
                    CreatedAt = b.CreatedAt
                }).ToList());

            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new AppUser { Id = id });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            sut = new BookingService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                notificationServiceMock.Object,
                userManagerMock.Object,
                loggerMock.Object,
                mailServiceMock.Object);
        }

        private static ProviderProfile CreateApprovedProvider()
        {
            return new ProviderProfile
            {
                Id = ProviderId,
                userId = ProviderUserId,
                ServiceName = "Approved Provider",
                verificationStatus = VerificationStatus.Approved
            };
        }

        private static Booking CreatePendingBooking()
        {
            return new Booking
            {
                Id = Guid.NewGuid(),
                UserId = PatientId,
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1),
                Status = BookingStatus.Pending,
                ProviderProfile = CreateApprovedProvider()
            };
        }

        [Fact]
        public async Task CreateBookingAsync_ApprovedProviderAvailableSlot_ReturnsPendingStatus()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking)null);

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
            notificationServiceMock.Verify(n => n.SendToUserAsync(PatientId, It.Is<CreateNotificationDto>(d => d.Title == "Booking Request Pending")), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(ProviderUserId, It.Is<CreateNotificationDto>(d => d.Title == "New Booking Request")), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.Is<CreateNotificationDto>(d => d.Title == "Booking Confirmed")), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_UnapprovedProvider_ThrowsBadRequest()
        {
            var provider = CreateApprovedProvider();
            provider.verificationStatus = VerificationStatus.Pending;

            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);

            await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateBookingAsync(dto, PatientId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_ConflictingSlot_ThrowsConflict()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new Booking { Id = Guid.NewGuid(), Status = BookingStatus.Pending });

            await Assert.ThrowsAsync<ConflictException>(() => sut.CreateBookingAsync(dto, PatientId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_RejectedBookingAtSameTimestamp_DoesNotBlockNewBooking()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            var rejectedBooking = new Booking
            {
                Id = Guid.NewGuid(),
                ProviderProfileId = ProviderId,
                BookingDate = dto.BookingDate,
                Status = BookingStatus.Rejected
            };

            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> specification) =>
                {
                    var matches = specification.Criteria == null
                        || specification.Criteria.Compile()(rejectedBooking);
                    return Task.FromResult(matches ? rejectedBooking : null);
                });

            // With the consistency fix, a rejected exact-timestamp booking is not a conflict.
            // The service still starts a transaction and creates a pending booking.
            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_EmptyProviderId_ThrowsBadRequest()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = Guid.Empty,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateBookingAsync(dto, PatientId));
        }

        [Fact]
        public async Task CreateBookingAsync_DatabaseUniqueConflictWithEmptyEntries_ThrowsOriginalException()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking)null);

            var dbException = new DbUpdateException(
                "duplicate",
                new Exception("IX_Bookings_ProviderProfileId_BookingDate duplicate"));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(dbException);

            await Assert.ThrowsAsync<DbUpdateException>(() => sut.CreateBookingAsync(dto, PatientId));
            unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_UnrelatedDatabaseFailure_ThrowsOriginalException()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking)null);

            var dbException = new DbUpdateException("unrelated", new Exception("some other failure"));
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ThrowsAsync(dbException);

            var thrown = await Assert.ThrowsAsync<DbUpdateException>(() => sut.CreateBookingAsync(dto, PatientId));
            Assert.Same(dbException, thrown);
        }

        [Fact]
        public async Task ApproveBookingAsync_OwningProvider_PersistsConfirmedAndNotifiesPatient()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            var result = await sut.ApproveBookingAsync(booking.Id, ProviderUserId);

            Assert.Equal(BookingStatus.Confirmed, result.Status);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(PatientId, It.Is<CreateNotificationDto>(d => d.Title == "Booking Confirmed")), Times.Once);
        }

        [Fact]
        public async Task ApproveBookingAsync_NonOwner_ThrowsForbidden()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.ApproveBookingAsync(booking.Id, "other-provider"));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task ApproveBookingAsync_PatientCannotApprove()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.ApproveBookingAsync(booking.Id, PatientId));
        }

        [Fact]
        public async Task ApproveBookingAsync_AlreadyConfirmed_DoesNotNotify()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Confirmed;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.ApproveBookingAsync(booking.Id, ProviderUserId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task RejectBookingAsync_OwningProvider_PersistsRejectedAndNotifiesPatient()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await sut.RejectBookingAsync(booking.Id, ProviderUserId);

            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(PatientId, It.Is<CreateNotificationDto>(d => d.Title == "Booking Rejected")), Times.Once);
        }

        [Fact]
        public async Task RejectBookingAsync_NonOwner_ThrowsForbidden()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.RejectBookingAsync(booking.Id, "other-provider"));
        }

        [Fact]
        public async Task CreateBookingAsync_NotificationFailureAfterCommit_StillReturnsPendingBooking()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking)null);

            notificationServiceMock.Setup(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()))
                .ThrowsAsync(new Exception("signalr down"));

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task ApproveBookingAsync_NotificationFailureAfterCommit_StillReturnsConfirmedBooking()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            notificationServiceMock.Setup(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()))
                .ThrowsAsync(new Exception("signalr down"));

            var result = await sut.ApproveBookingAsync(booking.Id, ProviderUserId);

            Assert.Equal(BookingStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task ApproveBookingAsync_ConcurrencyConflict_ThrowsConflictAndDoesNotNotify()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            await Assert.ThrowsAsync<ConflictException>(() => sut.ApproveBookingAsync(booking.Id, ProviderUserId));
            unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(), Times.Never);
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task RejectBookingAsync_ConcurrencyConflict_ThrowsConflictAndDoesNotNotify()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            await Assert.ThrowsAsync<ConflictException>(() => sut.RejectBookingAsync(booking.Id, ProviderUserId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CancelBookingAsync_ConcurrencyConflict_ThrowsConflictAndDoesNotNotify()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CancelBookingAsync_CompletedBooking_ThrowsConflict()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Completed;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CancelBookingAsync_RejectedBooking_ThrowsConflict()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Rejected;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId));
        }

        [Fact]
        public async Task CancelBookingAsync_CancelledBooking_ThrowsConflictAndNoNotifications()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Cancelled;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId));
            notificationServiceMock.Verify(n => n.SendToUserAsync(It.IsAny<string>(), It.IsAny<CreateNotificationDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateBookingAsync_PatientNotificationFailure_DoesNotPreventProviderNotification()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking)null);

            notificationServiceMock
                .Setup(n => n.SendToUserAsync(PatientId, It.IsAny<CreateNotificationDto>()))
                .ThrowsAsync(new Exception("patient notification delivery failed"));
            notificationServiceMock
                .Setup(n => n.SendToUserAsync(ProviderUserId, It.IsAny<CreateNotificationDto>()))
                .Returns(Task.CompletedTask);

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
            notificationServiceMock.Verify(n => n.SendToUserAsync(PatientId, It.IsAny<CreateNotificationDto>()), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(ProviderUserId, It.IsAny<CreateNotificationDto>()), Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_PatientNotificationFailure_DoesNotPreventProviderNotification()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            notificationServiceMock
                .Setup(n => n.SendToUserAsync(PatientId, It.IsAny<CreateNotificationDto>()))
                .ThrowsAsync(new Exception("patient notification delivery failed"));
            notificationServiceMock
                .Setup(n => n.SendToUserAsync(ProviderUserId, It.IsAny<CreateNotificationDto>()))
                .Returns(Task.CompletedTask);

            await sut.CancelBookingAsync(booking.Id, PatientId);

            notificationServiceMock.Verify(n => n.SendToUserAsync(PatientId, It.IsAny<CreateNotificationDto>()), Times.Once);
            notificationServiceMock.Verify(n => n.SendToUserAsync(ProviderUserId, It.IsAny<CreateNotificationDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateNotesAsync_NullNotes_PreservesExistingNotes()
        {
            var booking = CreatePendingBooking();
            booking.UserId = PatientId;
            booking.Notes = "existing note";
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await sut.UpdateNotesAsync(booking.Id, new UpdateBookingDto { Notes = null }, PatientId);

            Assert.Equal("existing note", booking.Notes);
        }

        [Fact]
        public async Task GetBookingAsync_AnotherPatientsBooking_ThrowsForbidden()
        {
            var booking = CreatePendingBooking();
            booking.UserId = "other-patient";
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetBookingAsync(booking.Id, PatientId));
        }

        [Fact]
        public async Task GetProviderBookingsAsync_AnotherProvidersBookings_ThrowsForbidden()
        {
            var otherProvider = new ProviderProfile
            {
                Id = ProviderId,
                userId = "other-provider",
                ServiceName = "Other",
                verificationStatus = VerificationStatus.Approved
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(otherProvider);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetProviderBookingsAsync(ProviderId, PatientId, 1, 10));
        }

        [Fact]
        public async Task ExpireOverdueBookingsAsync_ExpiredPendingBooking_BecomesExpired()
        {
            var booking = CreatePendingBooking();
            booking.CreatedAt = DateTime.UtcNow.AddHours(-9);
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new List<Booking> { booking });

            var expiredCount = await sut.ExpireOverdueBookingsAsync();

            Assert.Equal(1, expiredCount);
            Assert.Equal(BookingStatus.Expired, booking.Status);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ApproveBookingAsync_BeforeDeadline_BecomesConfirmed()
        {
            var booking = CreatePendingBooking();
            booking.CreatedAt = DateTime.UtcNow;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            var result = await sut.ApproveBookingAsync(booking.Id, ProviderUserId);

            Assert.Equal(BookingStatus.Confirmed, result.Status);
        }

        [Fact]
        public async Task ApproveBookingAsync_AfterDeadline_ThrowsConflictAndMarksExpired()
        {
            var booking = CreatePendingBooking();
            booking.CreatedAt = DateTime.UtcNow.AddHours(-9);
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.ApproveBookingAsync(booking.Id, ProviderUserId));
            Assert.Equal(BookingStatus.Expired, booking.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ExpiredPendingSlot_DoesNotBlockNewBooking()
        {
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());

            var expiredBooking = new Booking
            {
                Id = Guid.NewGuid(),
                ProviderProfileId = ProviderId,
                BookingDate = dto.BookingDate,
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-9)
            };

            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> specification) =>
                {
                    var matches = specification.Criteria == null
                        || specification.Criteria.Compile()(expiredBooking);
                    return Task.FromResult(matches ? expiredBooking : null);
                });

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CheckAvailabilityAsync_ExpiredPendingSlot_IsAvailable()
        {
            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(CreateApprovedProvider());

            var expiredBooking = new Booking
            {
                Id = Guid.NewGuid(),
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1),
                Status = BookingStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddHours(-9)
            };

            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> specification) =>
                {
                    var matches = specification.Criteria == null
                        || specification.Criteria.Compile()(expiredBooking);
                    return Task.FromResult(matches ? expiredBooking : null);
                });

            var result = await sut.CheckAvailabilityAsync(ProviderId, expiredBooking.BookingDate);

            Assert.True(result);
        }

        [Fact]
        public async Task ExpireOverdueBookingsAsync_DoesNotAffectConfirmedBooking()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Confirmed;
            booking.CreatedAt = DateTime.UtcNow.AddHours(-9);
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new List<Booking>());

            var expiredCount = await sut.ExpireOverdueBookingsAsync();

            Assert.Equal(0, expiredCount);
            Assert.Equal(BookingStatus.Confirmed, booking.Status);
        }

        [Fact]
        public async Task ExpireOverdueBookingsAsync_ConcurrentApprovalWins_DoesNotOverwriteConfirmedBooking()
        {
            var booking = CreatePendingBooking();
            booking.CreatedAt = DateTime.UtcNow.AddHours(-9);
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new List<Booking> { booking });
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            var expiredCount = await sut.ExpireOverdueBookingsAsync();

            Assert.Equal(0, expiredCount);
            Assert.Equal(BookingStatus.Pending, booking.Status);
        }

        [Fact]
        public async Task CancelBookingAsync_ExpiredBooking_ThrowsConflict()
        {
            var booking = CreatePendingBooking();
            booking.Status = BookingStatus.Expired;
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId));
        }

        [Fact]
        public async Task ApproveBookingAsync_ConcurrencyConflict_ThrowsConflict()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            await Assert.ThrowsAsync<ConflictException>(() => sut.ApproveBookingAsync(booking.Id, ProviderUserId));
        }

        [Fact]
        public async Task GetProviderBookingStatisticsAsync_UsesDatabaseCounts()
        {
            var provider = new ProviderProfile { Id = ProviderId, userId = ProviderUserId };
            providerRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(provider);
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((ISpecification<Booking> spec) =>
                {
                    if (spec is ProviderTodayBookingsSpecification) return 2;
                    if (spec is ProviderNextSevenDaysBookingsSpecification) return 3;
                    if (spec is ProviderPendingBookingsSpecification) return 4;
                    if (spec is ProviderCancelledBookingsSpecification) return 5;
                    return 0;
                });

            var result = await sut.GetProviderBookingStatisticsAsync(ProviderUserId);

            Assert.Equal(2, result.TodayCount);
            Assert.Equal(3, result.NextSevenDaysCount);
            Assert.Equal(4, result.PendingCount);
            Assert.Equal(5, result.CancelledCount);
            bookingRepositoryMock.Verify(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()), Times.Exactly(4));
        }

        [Fact]
        public async Task CreateBookingAsync_NeverConfiguredProvider_AllowsBooking()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = false;
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = DateTime.UtcNow.AddDays(1)
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking?)null);

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderOutsideWorkingHours_ThrowsBadRequest()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(11);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability
                    {
                        ProviderProfileId = ProviderId,
                        DayOfWeek = bookingDate.DayOfWeek,
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(10)
                    }
                });

            await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateBookingAsync(dto, PatientId));
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderInsideWorkingHours_ReturnsPending()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(9).AddMinutes(30);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking?)null);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability
                    {
                        ProviderProfileId = ProviderId,
                        DayOfWeek = bookingDate.DayOfWeek,
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(10)
                    }
                });

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderDayOff_ThrowsBadRequest()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>());

            await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateBookingAsync(dto, PatientId));
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderWrongWeekday_ThrowsBadRequest()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability
                    {
                        ProviderProfileId = ProviderId,
                        DayOfWeek = bookingDate.DayOfWeek == DayOfWeek.Monday ? DayOfWeek.Tuesday : DayOfWeek.Monday,
                        StartTime = TimeSpan.FromHours(9),
                        EndTime = TimeSpan.FromHours(10)
                    }
                });

            await Assert.ThrowsAsync<BadRequestException>(() => sut.CreateBookingAsync(dto, PatientId));
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderMultipleIntervals_ReturnsPending()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(11).AddMinutes(30);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking?)null);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability { ProviderProfileId = ProviderId, DayOfWeek = bookingDate.DayOfWeek, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) },
                    new ProviderAvailability { ProviderProfileId = ProviderId, DayOfWeek = bookingDate.DayOfWeek, StartTime = TimeSpan.FromHours(11), EndTime = TimeSpan.FromHours(12) }
                });

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ConfiguredProviderBoundaryStart_ReturnsPending()
        {
            var provider = CreateApprovedProvider();
            provider.HasAvailabilityConfigured = true;
            var bookingDate = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
            var dto = new CreateBookingDto
            {
                ProviderProfileId = ProviderId,
                BookingDate = bookingDate
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(provider);
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync((Booking?)null);
            availabilityRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability { ProviderProfileId = ProviderId, DayOfWeek = bookingDate.DayOfWeek, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(10) }
                });

            var result = await sut.CreateBookingAsync(dto, PatientId);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }

        [Fact]
        public async Task GetProviderBookingsAsync_FilterUpcoming_ReturnsOnlyUpcomingBookings()
        {
            var provider = CreateApprovedProvider();
            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId)).ReturnsAsync(provider);

            var upcoming = CreatePendingBooking();
            upcoming.BookingDate = DateTime.UtcNow.AddDays(1);
            var cancelled = CreatePendingBooking();
            cancelled.Id = Guid.NewGuid();
            cancelled.BookingDate = DateTime.UtcNow.AddDays(2);
            cancelled.Status = BookingStatus.Cancelled;

            var bookings = new List<Booking> { upcoming, cancelled };
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec)));
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec).Count));

            var result = await sut.GetProviderBookingsAsync(ProviderId, ProviderUserId, 1, 10, "upcoming", null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(upcoming.Id, result.Data[0].Id);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task GetProviderBookingsAsync_DateRange_ReturnsBoundedBookings()
        {
            var provider = CreateApprovedProvider();
            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId)).ReturnsAsync(provider);

            var first = CreatePendingBooking();
            first.BookingDate = DateTime.UtcNow.AddDays(1);
            var second = CreatePendingBooking();
            second.Id = Guid.NewGuid();
            second.BookingDate = DateTime.UtcNow.AddDays(5);

            var bookings = new List<Booking> { first, second };
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec)));
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec).Count));

            var result = await sut.GetProviderBookingsAsync(
                ProviderId,
                ProviderUserId,
                1,
                10,
                null,
                DateTime.UtcNow.AddDays(2),
                DateTime.UtcNow.AddDays(6),
                null);

            Assert.Single(result.Data);
            Assert.Equal(second.Id, result.Data[0].Id);
        }

        [Fact]
        public async Task GetProviderBookingsAsync_SearchByPatientName_ReturnsMatchingBooking()
        {
            var provider = CreateApprovedProvider();
            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId)).ReturnsAsync(provider);

            var matching = CreatePendingBooking();
            matching.BookingDate = DateTime.UtcNow.AddDays(1);
            matching.User = new AppUser { Id = PatientId, FullName = "Alice Patient", Email = "alice@example.com" };
            var other = CreatePendingBooking();
            other.Id = Guid.NewGuid();
            other.BookingDate = DateTime.UtcNow.AddDays(2);
            other.User = new AppUser { Id = "other", FullName = "Bob Patient", Email = "bob@example.com" };

            var bookings = new List<Booking> { matching, other };
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec)));
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(FilterBookings(bookings, spec).Count));

            var result = await sut.GetProviderBookingsAsync(ProviderId, ProviderUserId, 1, 10, null, null, null, "Alice");

            Assert.Single(result.Data);
            Assert.Equal(matching.Id, result.Data[0].Id);
        }

        [Fact]
        public async Task GetProviderBookingsAsync_Pagination_ReturnsPageAndTotalCount()
        {
            var provider = CreateApprovedProvider();
            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId)).ReturnsAsync(provider);

            var first = CreatePendingBooking();
            first.BookingDate = DateTime.UtcNow.AddDays(1);
            var second = CreatePendingBooking();
            second.Id = Guid.NewGuid();
            second.BookingDate = DateTime.UtcNow.AddDays(2);

            var bookings = new List<Booking> { first, second };
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) =>
                {
                    var filtered = bookings.Where(b => spec.Criteria == null || spec.Criteria.Compile()(b)).ToList();
                    if (spec.IsPagingEnabled)
                        filtered = filtered.Skip(spec.Skip).Take(spec.Take).ToList();
                    return Task.FromResult<IReadOnlyList<Booking>>(filtered);
                });
            bookingRepositoryMock.Setup(r => r.CountAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) => Task.FromResult(bookings.Count(b => spec.Criteria == null || spec.Criteria.Compile()(b))));

            var result = await sut.GetProviderBookingsAsync(ProviderId, ProviderUserId, 1, 1, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(1, result.PageSize);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task CancelBookingAsync_WithReason_PersistsCancellationReason()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await sut.CancelBookingAsync(booking.Id, PatientId, "Patient requested cancellation");

            Assert.Equal(BookingStatus.Cancelled, booking.Status);
            Assert.Equal("Patient requested cancellation", booking.CancellationReason);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelBookingAsync_PersistenceFailure_DoesNotSendEmail()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new DbUpdateConcurrencyException("concurrency conflict"));

            await Assert.ThrowsAsync<ConflictException>(() => sut.CancelBookingAsync(booking.Id, PatientId, "Reason"));

            mailServiceMock.Verify(m => m.sendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task CancelBookingAsync_Success_SendsCancellationEmail()
        {
            var booking = CreatePendingBooking();
            booking.User = new AppUser { Id = PatientId, Email = "patient@example.com" };
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);
            mailServiceMock.Setup(m => m.sendMail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            await sut.CancelBookingAsync(booking.Id, PatientId, "Reason");

            mailServiceMock.Verify(m => m.sendMail("patient@example.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CancelBookingsAsync_MixedValidAndInvalid_ReturnsCancelledAndRejectedIds()
        {
            var validBooking = CreatePendingBooking();
            var invalidBooking = CreatePendingBooking();
            invalidBooking.Id = Guid.NewGuid();
            invalidBooking.Status = BookingStatus.Completed;

            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .Returns((ISpecification<Booking> spec) =>
                {
                    var compiled = spec.Criteria?.Compile();
                    if (compiled != null && compiled(validBooking)) return Task.FromResult<Booking?>(validBooking);
                    if (compiled != null && compiled(invalidBooking)) return Task.FromResult<Booking?>(invalidBooking);
                    return Task.FromResult<Booking?>(null);
                });

            var result = await sut.CancelBookingsAsync(new[] { validBooking.Id, invalidBooking.Id }, "Reason", PatientId);

            Assert.Contains(validBooking.Id, result.CancelledBookingIds);
            Assert.Contains(invalidBooking.Id, result.RejectedBookingIds);
        }

        [Fact]
        public async Task CancelBookingsAsync_DuplicateIds_ProcessesOnce()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            var result = await sut.CancelBookingsAsync(new[] { booking.Id, booking.Id }, "Reason", PatientId);

            Assert.Single(result.CancelledBookingIds);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelBookingsAsync_OtherProviderBooking_RejectsBooking()
        {
            var booking = CreatePendingBooking();
            booking.UserId = "other-patient";
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            var result = await sut.CancelBookingsAsync(new[] { booking.Id }, "Reason", PatientId);

            Assert.Empty(result.CancelledBookingIds);
            Assert.Contains(booking.Id, result.RejectedBookingIds);
        }

        private static IReadOnlyList<Booking> FilterBookings(List<Booking> bookings, ISpecification<Booking> spec)
        {
            var filtered = spec.Criteria == null
                ? bookings
                : bookings.Where(b => spec.Criteria.Compile()(b)).ToList();

            if (spec.IsPagingEnabled)
                filtered = filtered.Skip(spec.Skip).Take(spec.Take).ToList();

            return filtered.AsReadOnly();
        }
    }
}
