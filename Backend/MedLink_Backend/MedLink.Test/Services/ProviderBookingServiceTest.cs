using System;
using AutoMapper;
using MedLink.Application.DTOs.Common.BookingDTOs;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedLink.Test.Services
{
    public class ProviderBookingServiceTest
    {
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<ILogger<ProviderBookingService>> loggerMock = new();
        private readonly Mock<UserManager<AppUser>> userManagerMock;
        private readonly Mock<INotificationService> notificationServiceMock = new();
        private readonly Mock<ICacheService> cacheServiceMock = new();
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IGenaricRepo<Booking>> bookingRepositoryMock = new();
        private readonly Mock<IGenaricRepo<ProviderProfile>> providerRepositoryMock = new();

        private readonly ProviderBookingService sut;

        private static readonly Guid ProviderId = Guid.NewGuid();
        private const string ProviderUserId = "provider-user";
        private const string PatientId = "patient-user";

        public ProviderBookingServiceTest()
        {
            var storeMock = new Mock<IUserStore<AppUser>>();
            userManagerMock = new Mock<UserManager<AppUser>>(
                storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            unitOfWorkMock.Setup(u => u.Repository<Booking>()).Returns(bookingRepositoryMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderProfile>()).Returns(providerRepositoryMock.Object);
            bookingRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(new List<Booking>());
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);

            cacheServiceMock.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            // GetOrSetAsync just invokes the factory directly, bypassing the real cache,
            // which is enough to exercise ProviderBookingService's own logic in isolation.
            cacheServiceMock
                .Setup(c => c.GetOrSetAsync<PaginatedResultDto<GetBookingDto>>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<PaginatedResultDto<GetBookingDto>?>>>(),
                    null,
                    null))
                .Returns<string, Func<Task<PaginatedResultDto<GetBookingDto>?>>, TimeSpan?, TimeSpan?>(
                    (_, factory, _, _) => factory());
            cacheServiceMock
                .Setup(c => c.GetOrSetAsync<ProviderBookingStatisticsDto>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ProviderBookingStatisticsDto?>>>(),
                    null,
                    null))
                .Returns<string, Func<Task<ProviderBookingStatisticsDto?>>, TimeSpan?, TimeSpan?>(
                    (_, factory, _, _) => factory());

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

            userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => new AppUser { Id = id });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            sut = new ProviderBookingService(
                unitOfWorkMock.Object,
                mapperMock.Object,
                notificationServiceMock.Object,
                userManagerMock.Object,
                cacheServiceMock.Object,
                loggerMock.Object);
        }

        private static ProviderProfile CreateApprovedProvider()
        {
            return new ProviderProfile
            {
                Id = ProviderId,
                userId = ProviderUserId,
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
        public async Task GetBookingsByProviderIdAsync_AnotherProvidersBookings_ThrowsForbidden()
        {
            var otherProvider = new ProviderProfile
            {
                Id = ProviderId,
                userId = "other-provider",
                verificationStatus = VerificationStatus.Approved
            };

            providerRepositoryMock.Setup(r => r.GetByIdAsync(ProviderId))
                .ReturnsAsync(otherProvider);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetBookingsByProviderIdAsync(ProviderId, PatientId, 1, 10));
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
        public async Task GetBookingByIdAsync_NonOwner_ThrowsForbidden()
        {
            var booking = CreatePendingBooking();
            bookingRepositoryMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<Booking>>()))
                .ReturnsAsync(booking);

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.GetBookingByIdAsync(booking.Id, "other-provider"));
        }
    }
}
