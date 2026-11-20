using AutoMapper;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.ProviderManagement;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Common;
using System.Linq;
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
            IUnitOfWork unitOfWork,
            IMailService? mailService = null
            )
        {
            this.cacheService = cacheService;
            this.logger = logger;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.mailService = mailService;
        }
        private readonly IUnitOfWork unitOfWork;
        private readonly  ICacheService cacheService;
        private readonly ILogger<AdminService> logger;
        private readonly IMapper mapper;
        private readonly IMailService? mailService;
        public async Task<PaginatedResultDto<GetProviderRequestDto>>
    GetAllProviderRequest(int pageNum, int pageSize, string? search = null, VerificationStatus? status = null)
        {
            if (pageNum < 1 || pageSize < 1)
                throw new ArgumentException(
                    "Page number and page size must be greater than zero.");

            logger.LogInformation(
                "Attempting to get provider requests. Page: {PageNum}, " +
                "PageSize: {PageSize}, Search: {Search}, Status: {Status}",
                pageNum,
                pageSize,
                search,
                status);

            var cacheKey =
                $"Get_ProviderRequest_pageNum:{pageNum}_pageSize:{pageSize}_search:{search}_status:{status}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var specification =
                    new ProviderStatusSpecification(pageNum, pageSize, search, status);

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

        public async Task<IReadOnlyList<GetProviderDocumentDto>> GetProviderDocumentsAsync(Guid providerId)
        {
            var files = await unitOfWork.Repository<StoredFile>()
                .GetAllAsync(new StoredFilesByProviderSpecification(providerId));
            return mapper.Map<IReadOnlyList<GetProviderDocumentDto>>(files);
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

            if (mailService != null && user?.Email != null)
            {
                try
                {
                    await mailService.sendMail(
                        user.Email,
                        "Provider application approved",
                        "Your MedLink provider application has been approved. You can now sign in and manage your provider profile.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send provider approval email to {Email}.", user.Email);
                }
            }
        }
        public async Task RejectProviderAsync(Guid providerId, string? rejectionReason = null)
        {
            if (rejectionReason != null && rejectionReason.Length > 1000)
                throw new BadRequestException("Rejection reason cannot exceed 1000 characters.");

            logger.LogInformation(
                "Rejecting provider request for ProviderId: {ProviderId}",
                providerId);

            var provider = await unitOfWork
                .Repository<ProviderProfile>()
                .GetByIdAsync(providerId);

            if (provider == null)
                throw new NotFoundException($"Provider with id {providerId} not found.");

            provider.verificationStatus = VerificationStatus.Rejected;
            provider.RejectionReason = rejectionReason;
            unitOfWork.Repository<ProviderProfile>().Update(provider);

            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync("Get_ProviderRequest_pageNum:");

            var user = await unitOfWork.Repository<AppUser>().GetByIdAsync(provider.userId);
            if (mailService != null && user?.Email != null)
            {
                try
                {
                    var reasonText = string.IsNullOrWhiteSpace(rejectionReason)
                        ? "No reason was provided."
                        : rejectionReason;
                    await mailService.sendMail(
                        user.Email,
                        "Provider application rejected",
                        $"Your MedLink provider application was rejected. Reason: {reasonText}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to send provider rejection email to {Email}.", user.Email);
                }
            }
        }
    }
}
