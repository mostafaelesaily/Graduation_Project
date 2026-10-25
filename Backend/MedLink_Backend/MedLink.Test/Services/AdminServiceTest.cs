using AutoMapper;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedLink.Test.Services
{
    public class AdminServiceTest
    {
        private readonly Mock<ILogger<AdminService>> loggerMock = new();
        private readonly Mock<ICacheService> cacheServiceMock = new();
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<IUnitOfWork> unitOfWorkMock = new();
        private readonly Mock<IGenaricRepo<ProviderProfile>> providerRepoMock = new();
        private readonly Mock<IGenaricRepo<AppUser>> userRepoMock = new();
        private readonly Mock<IGenaricRepo<StoredFile>> storedFileRepoMock = new();
        private readonly AdminService sut;

        public AdminServiceTest()
        {
            unitOfWorkMock.Setup(u => u.Repository<ProviderProfile>()).Returns(providerRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<AppUser>()).Returns(userRepoMock.Object);
            unitOfWorkMock.Setup(u => u.Repository<StoredFile>()).Returns(storedFileRepoMock.Object);

            // GetOrSetAsync just invokes the factory directly, bypassing the real cache,
            // which is enough to exercise AdminService's own logic in isolation.
            cacheServiceMock
                .Setup(c => c.GetOrSetAsync<PaginatedResultDto<GetProviderRequestDto>>(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<PaginatedResultDto<GetProviderRequestDto>?>>>(),
                    null,
                    null))
                .Returns<string, Func<Task<PaginatedResultDto<GetProviderRequestDto>?>>, TimeSpan?, TimeSpan?>(
                    (_, factory, _, _) => factory());

            sut = new AdminService(
                loggerMock.Object,
                cacheServiceMock.Object,
                mapperMock.Object,
                unitOfWorkMock.Object);
        }

        private static ProviderProfile CreateProvider(VerificationStatus status = VerificationStatus.Pending)
        {
            return new ProviderProfile
            {
                Id = Guid.NewGuid(),
                userId = "user-1",
                ProviderName = "Al-Noor Lab",
                verificationStatus = status
            };
        }

        // ---------- GetAllProviderRequest ----------

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(-1, 10)]
        public async Task GetAllProviderRequest_InvalidPaging_ThrowsArgumentException(int pageNum, int pageSize)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => sut.GetAllProviderRequest(pageNum, pageSize));
        }

        [Fact]
        public async Task GetAllProviderRequest_ValidPaging_ReturnsPaginatedResult()
        {
            var provider = CreateProvider();
            provider.LicenseFileId = Guid.NewGuid();
            var providers = new List<ProviderProfile> { provider };
            var dtos = new List<GetProviderRequestDto>
            {
                new GetProviderRequestDto { ProviderId = provider.Id, ProviderName = provider.ProviderName }
            };
            var storedFile = new StoredFile { Id = provider.LicenseFileId, FileName = "license.pdf", FilePath = "ProviderLicenses/license.pdf", FileType = "application/pdf" };

            providerRepoMock
                .Setup(r => r.GetAllAsync(It.IsAny<ISpecification<ProviderProfile>>()))
                .ReturnsAsync(providers);
            mapperMock
                .Setup(m => m.Map<IReadOnlyList<GetProviderRequestDto>>(providers))
                .Returns(dtos);
            storedFileRepoMock
                .Setup(r => r.GetByIdAsync(provider.LicenseFileId))
                .ReturnsAsync(storedFile);

            var result = await sut.GetAllProviderRequest(1, 10);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
            Assert.Same(dtos, result.Data);
            Assert.Equal("ProviderLicenses/license.pdf", dtos[0].LicenseImageUrl);
        }

        // ---------- AcceptProviderAsync ----------

        [Fact]
        public async Task AcceptProviderAsync_ProviderNotFound_ThrowsNotFoundException()
        {
            var providerId = Guid.NewGuid();
            providerRepoMock.Setup(r => r.GetByIdAsync(providerId)).ReturnsAsync((ProviderProfile?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => sut.AcceptProviderAsync(providerId));
        }

        [Fact]
        public async Task AcceptProviderAsync_ProviderFound_ApprovesProviderAndActivatesUser()
        {
            var provider = CreateProvider();
            var user = new AppUser { Id = provider.userId, IsActice = false };

            providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id)).ReturnsAsync(provider);
            userRepoMock.Setup(r => r.GetByIdAsync(provider.userId)).ReturnsAsync(user);

            await sut.AcceptProviderAsync(provider.Id);

            Assert.Equal(VerificationStatus.Approved, provider.verificationStatus);
            Assert.True(user.IsActice);
            providerRepoMock.Verify(r => r.Update(provider), Times.Once);
            userRepoMock.Verify(r => r.Update(user), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            cacheServiceMock.Verify(c => c.RemoveAsync("Get_ProviderRequest_pageNum:"), Times.Once);
        }

        [Fact]
        public async Task AcceptProviderAsync_UserNotFound_StillApprovesProviderWithoutThrowing()
        {
            var provider = CreateProvider();
            providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id)).ReturnsAsync(provider);
            userRepoMock.Setup(r => r.GetByIdAsync(provider.userId)).ReturnsAsync((AppUser?)null);

            await sut.AcceptProviderAsync(provider.Id);

            Assert.Equal(VerificationStatus.Approved, provider.verificationStatus);
            userRepoMock.Verify(r => r.Update(It.IsAny<AppUser>()), Times.Never);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ---------- RejectProviderAsync ----------

        [Fact]
        public async Task RejectProviderAsync_ProviderNotFound_ThrowsNotFoundException()
        {
            var providerId = Guid.NewGuid();
            providerRepoMock.Setup(r => r.GetByIdAsync(providerId)).ReturnsAsync((ProviderProfile?)null);

            await Assert.ThrowsAsync<NotFoundException>(() => sut.RejectProviderAsync(providerId));
        }

        [Fact]
        public async Task RejectProviderAsync_ProviderFound_RejectsProvider()
        {
            var provider = CreateProvider();
            providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id)).ReturnsAsync(provider);

            await sut.RejectProviderAsync(provider.Id);

            Assert.Equal(VerificationStatus.Rejected, provider.verificationStatus);
            providerRepoMock.Verify(r => r.Update(provider), Times.Once);
            unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            cacheServiceMock.Verify(c => c.RemoveAsync("Get_ProviderRequest_pageNum:"), Times.Once);
        }
    }
}
