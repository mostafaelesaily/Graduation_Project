using System;
using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
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
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IGenaricRepo<Booking>> bookingRepositoryMock = new();
        private readonly Mock<IGenaricRepo<ProviderProfile>> providerRepositoryMock = new();

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
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            mapperMock.Setup(m => m.Map<BookingDto>(It.IsAny<Booking>()))
                .Returns((Booking booking) => new BookingDto
                {
                    Id = booking.Id,
                    UserId = booking.UserId,
                    ProviderProfileId = booking.ProviderProfileId,
                    BookingDate = booking.BookingDate,
                    Status = booking.Status,
                    Notes = booking.Notes,
                    CreatedAt = booking.CreatedAt
                });

            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new AppUser { Id = id });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            sut = new BookingService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                notificationServiceMock.Object,
                userManagerMock.Object,
                loggerMock.Object);
        }

        private static ProviderProfile CreateApprovedProvider()
        {
            return new ProviderProfile
            {
                Id = ProviderId,
                userId = ProviderUserId,
                ProviderName = "Approved Provider",
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

            var result = await sut.RejectBookingAsync(booking.Id, ProviderUserId);

            Assert.Equal(BookingStatus.Rejected, result.Status);
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

            var result = await sut.UpdateNotesAsync(booking.Id, new UpdateBookingDto { Notes = null }, PatientId);

            Assert.Equal("existing note", result.Notes);
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
                ProviderName = "Other",
                verificationStatus = VerificationStatus.Approved
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(otherProvider);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetProviderBookingsAsync(ProviderId, PatientId));
        }
    }
}
