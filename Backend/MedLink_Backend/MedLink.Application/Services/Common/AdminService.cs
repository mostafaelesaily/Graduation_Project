using AutoMapper;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Services.Common
{
    public class AdminService : IAdminService
    {
        public AdminService
            (
            ILogger<AdminService> logger ,
            ICacheService cacheService,
            IMapper mapper,
            IUnitOfWork unitOfWork
            )
        {
            this.cacheService = cacheService;
            this.logger = logger;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }
        private readonly IUnitOfWork unitOfWork;
        private readonly  ICacheService cacheService;
        private readonly ILogger<AdminService> logger;
        private readonly IMapper mapper;
        public async Task<PaginatedResultDto<GetProviderRequestDto>>
    GetAllProviderRequest(int pageNum, int pageSize)
        {
            if (pageNum < 1 || pageSize < 1)
                throw new ArgumentException(
                    "Page number and page size must be greater than zero.");

            logger.LogInformation(
                "Attempting to get provider requests. Page: {PageNum}, " +
                "PageSize: {PageSize}, Status: Pending or Rejected",
                pageNum,
                pageSize);

            var cacheKey =
                $"Get_ProviderRequest_pageNum:{pageNum}_pageSize:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var specification =
                    new ProviderStatusSpecification(pageNum, pageSize);

                var providers = await unitOfWork
                    .Repository<ProviderProfile>()
                    .GetAllAsync(specification);

                var providerDtos = mapper.Map<IReadOnlyList<GetProviderRequestDto>>(
                    providers);

                foreach (var (provider, dto) in providers.Zip(providerDtos))
                {
                    var licenseFile = await unitOfWork
                        .Repository<StoredFile>()
                        .GetByIdAsync(provider.LicenseFileId);

                    dto.LicenseImageUrl = licenseFile?.FilePath;
                }

                return new PaginatedResultDto<GetProviderRequestDto>
                {
                    Data = providerDtos,
                    PageNumber = pageNum,
                    PageSize = pageSize
                };
            });

            return result;
        }
        public async Task AcceptProviderAsync(Guid providerId)
        {
            logger.LogInformation(
                "Accepting provider request for ProviderId: {ProviderId}",
                providerId);

            var provider = await unitOfWork
                .Repository<ProviderProfile>()
                .GetByIdAsync(providerId);

            if (provider == null)
                throw new NotFoundException($"Provider with id {providerId} not found.");

            provider.verificationStatus = VerificationStatus.Approved;
            unitOfWork.Repository<ProviderProfile>().Update(provider);

            var user = await unitOfWork.Repository<AppUser>().GetByIdAsync(provider.userId);
            if (user != null)
            {
                user.IsActice = true;
                unitOfWork.Repository<AppUser>().Update(user);
            }

            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_ProviderRequest_pageNum:");
        }
        public async Task RejectProviderAsync(Guid providerId)
        {
            logger.LogInformation(
                "Rejecting provider request for ProviderId: {ProviderId}",
                providerId);

            var provider = await unitOfWork
                .Repository<ProviderProfile>()
                .GetByIdAsync(providerId);

            if (provider == null)
                throw new NotFoundException($"Provider with id {providerId} not found.");

            provider.verificationStatus = VerificationStatus.Rejected;
            unitOfWork.Repository<ProviderProfile>().Update(provider);

            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_ProviderRequest_pageNum:");
        }
    }
}
