using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedLink.Application.Services.Common
{
    public class ProviderAvailabilityService : IProviderAvailabilityService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;

        public ProviderAvailabilityService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
        }

        public async Task<IReadOnlyList<GetAvailabilityDto>> GetMyAvailabilityAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var items = await unitOfWork.Repository<ProviderAvailability>().GetAllAsync(
                new ProviderAvailabilitiesByProviderSpecification(profile.Id));
            var ordered = items
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .ToList();
            return mapper.Map<IReadOnlyList<GetAvailabilityDto>>(ordered);
        }

        public async Task SetMyAvailabilityAsync(string userId, EditAvailabilityDto dto)
        {
            if (dto is null)
                throw new BadRequestException("Availability payload is required.");

            var user = await userManager.FindByIdAsync(userId);
            if (user is null || !await userManager.IsInRoleAsync(user, MedLink.Application.Constants.Roles.Provider))
                throw new ForbiddenException("Provider access is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            if (profile.verificationStatus != VerificationStatus.Approved)
                throw new ForbiddenException("Provider is not approved.");

            var intervals = dto.Intervals ?? new List<GetAvailabilityDto>();

            foreach (var interval in intervals)
            {
                if (!Enum.IsDefined(typeof(DayOfWeek), interval.DayOfWeek))
                    throw new BadRequestException("Invalid day of week.");

                if (interval.StartTime >= interval.EndTime)
                    throw new BadRequestException("Start time must be earlier than end time.");
            }

            var grouped = intervals.GroupBy(i => i.DayOfWeek);
            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(i => i.StartTime).ToList();
                for (int i = 1; i < ordered.Count; i++)
                {
                    if (ordered[i].StartTime < ordered[i - 1].EndTime)
                        throw new BadRequestException("Availability intervals cannot overlap.");
                }
            }

            await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var existing = await unitOfWork.Repository<ProviderAvailability>().GetAllAsync(
                    new ProviderAvailabilitiesByProviderSpecification(profile.Id));

                foreach (var item in existing)
                {
                    unitOfWork.Repository<ProviderAvailability>().Delete(item);
                }

                foreach (var interval in intervals)
                {
                    var availability = mapper.Map<ProviderAvailability>(interval);
                    availability.ProviderProfileId = profile.Id;
                    await unitOfWork.Repository<ProviderAvailability>().AddAsync(availability);
                }

                await unitOfWork.SaveChangesAsync();
                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<ProviderProfile> GetProviderProfileByUserIdAsync(string userId)
        {
            var profile = await unitOfWork.Repository<ProviderProfile>().FindAsync(
                new ProviderProfileByUserSpecification(userId));
            if (profile is null)
                throw new NotFoundException("Provider profile not found.");
            return profile;
        }
    }
}