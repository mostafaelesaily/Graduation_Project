using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedLink.Test.Services
{
    public class ProviderAvailabilityServiceTest
    {
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<IGenaricRepo<ProviderProfile>> providerRepoMock = new();
        private readonly Mock<IGenaricRepo<ProviderAvailability>> availabilityRepoMock = new();
        private readonly Mock<UserManager<AppUser>> userManagerMock;

        private readonly ProviderAvailabilityService sut;

        private static readonly Guid ProviderId = Guid.NewGuid();
        private const string ProviderUserId = "provider-user";

        public ProviderAvailabilityServiceTest()
        {
            var storeMock = new Mock<IUserStore<AppUser>>();
            userManagerMock = new Mock<UserManager<AppUser>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            unitOfWorkMock.Setup(u => u.Repository<ProviderProfile>()).Returns(providerRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<ProviderAvailability>()).Returns(availabilityRepoMock.Object);
            unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
            unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            userManagerMock.Setup(x => x.FindByIdAsync(ProviderUserId))
                .ReturnsAsync(new AppUser { Id = ProviderUserId });
            userManagerMock.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "Provider"))
                .ReturnsAsync(true);

            mapperMock.Setup(m => m.Map<ProviderAvailability>(It.IsAny<GetAvailabilityDto>()))
                .Returns((GetAvailabilityDto dto) => new ProviderAvailability
                {
                    DayOfWeek = dto.DayOfWeek,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime
                });
            mapperMock.Setup(m => m.Map<IReadOnlyList<GetAvailabilityDto>>(It.IsAny<IEnumerable<ProviderAvailability>>()))
                .Returns((IEnumerable<ProviderAvailability> items) => items.Select(i => new GetAvailabilityDto
                {
                    DayOfWeek = i.DayOfWeek,
                    StartTime = i.StartTime,
                    EndTime = i.EndTime
                }).ToList());

            sut = new ProviderAvailabilityService(unitOfWorkMock.Object, mapperMock.Object, userManagerMock.Object);
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

        [Fact]
        public async Task GetMyAvailabilityAsync_ReturnsOrderedIntervals()
        {
            var provider = CreateApprovedProvider();
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(provider);
            availabilityRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability { ProviderProfileId = ProviderId, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(10), EndTime = TimeSpan.FromHours(12) },
                    new ProviderAvailability { ProviderProfileId = ProviderId, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(9) }
                });

            var result = await sut.GetMyAvailabilityAsync(ProviderUserId);

            Assert.Equal(2, result.Count);
            Assert.Equal(TimeSpan.FromHours(8), result[0].StartTime);
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_InvalidWeekday_ThrowsBadRequest()
        {
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(CreateApprovedProvider());

            var dto = new EditAvailabilityDto
            {
                Intervals = new List<GetAvailabilityDto>
                {
                    new GetAvailabilityDto { DayOfWeek = (DayOfWeek)99, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(9) }
                }
            };

            await Assert.ThrowsAsync<BadRequestException>(() => sut.SetMyAvailabilityAsync(ProviderUserId, dto));
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_StartAfterEnd_ThrowsBadRequest()
        {
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(CreateApprovedProvider());

            var dto = new EditAvailabilityDto
            {
                Intervals = new List<GetAvailabilityDto>
                {
                    new GetAvailabilityDto { DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(8) }
                }
            };

            await Assert.ThrowsAsync<BadRequestException>(() => sut.SetMyAvailabilityAsync(ProviderUserId, dto));
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_OverlappingIntervals_ThrowsBadRequest()
        {
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(CreateApprovedProvider());

            var dto = new EditAvailabilityDto
            {
                Intervals = new List<GetAvailabilityDto>
                {
                    new GetAvailabilityDto { DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(10) },
                    new GetAvailabilityDto { DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(9), EndTime = TimeSpan.FromHours(11) }
                }
            };

            await Assert.ThrowsAsync<BadRequestException>(() => sut.SetMyAvailabilityAsync(ProviderUserId, dto));
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_ValidSchedule_ReplacesExisting()
        {
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(CreateApprovedProvider());
            availabilityRepoMock.Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderAvailability>>()))
                .ReturnsAsync(new List<ProviderAvailability>
                {
                    new ProviderAvailability { Id = Guid.NewGuid(), ProviderProfileId = ProviderId, DayOfWeek = DayOfWeek.Monday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(9) }
                });

            var dto = new EditAvailabilityDto
            {
                Intervals = new List<GetAvailabilityDto>
                {
                    new GetAvailabilityDto { DayOfWeek = DayOfWeek.Tuesday, StartTime = TimeSpan.FromHours(8), EndTime = TimeSpan.FromHours(9) }
                }
            };

            await sut.SetMyAvailabilityAsync(ProviderUserId, dto);

            availabilityRepoMock.Verify(r => r.Delete(It.IsAny<ProviderAvailability>()), Times.Once);
            availabilityRepoMock.Verify(r => r.AddAsync(It.IsAny<ProviderAvailability>()), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetMyAvailabilityAsync_NotApprovedProvider_ThrowsForbidden()
        {
            var provider = CreateApprovedProvider();
            provider.verificationStatus = VerificationStatus.Pending;
            providerRepoMock.Setup(r => r.FindAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(provider);

            var dto = new EditAvailabilityDto
            {
                Intervals = new List<GetAvailabilityDto>()
            };

            await Assert.ThrowsAsync<ForbiddenException>(() => sut.SetMyAvailabilityAsync(ProviderUserId, dto));
        }
    }
}