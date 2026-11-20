using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Constants;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.ProviderManagement;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedLink.Application.Services.Common
{
    public class ProviderManagementService : IProviderManagementService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;
        private readonly IFileService fileService;
        private readonly ILogger<ProviderManagementService> logger;

        public ProviderManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager,
            IFileService fileService,
            ILogger<ProviderManagementService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.userManager = userManager;
            this.fileService = fileService;
            this.logger = logger;
        }

        private async Task<ProviderProfile> GetProviderProfileByUserIdAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null || !await userManager.IsInRoleAsync(user, Roles.Provider))
                throw new ForbiddenException("Provider access is required.");

            var profile = await unitOfWork.Repository<ProviderProfile>()
                .FindAsync(new ProviderProfileByUserSpecification(userId));
            if (profile == null)
                throw new NotFoundException("Provider profile not found.");
            return profile;
        }

        public async Task<GetProviderProfileDto> GetMyProfileAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            var dto = mapper.Map<GetProviderProfileDto>(profile);
            dto.IsActive = user.IsActice;
            return dto;
        }

        public async Task<GetProviderProfileDto> UpdateMyProfileAsync(string userId, UpdateProviderProfileDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Profile payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var trackedProfile = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(profile.Id);
            if (trackedProfile == null)
                throw new NotFoundException("Provider profile not found.");

            trackedProfile.FullName = dto.FullName;
            trackedProfile.ServiceName = dto.ServiceName;
            unitOfWork.Repository<ProviderProfile>().Update(trackedProfile);
            await unitOfWork.SaveChangesAsync();

            return await GetMyProfileAsync(userId);
        }

        public async Task<IReadOnlyList<GetProviderLocationDto>> GetMyLocationsAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var locations = await unitOfWork.Repository<ProviderLocations>()
                .GetAllAsync(new ProviderLocationsByProviderSpecification(profile.Id));
            return mapper.Map<IReadOnlyList<GetProviderLocationDto>>(locations);
        }

        public async Task<GetProviderLocationDto> CreateLocationAsync(string userId, CreateProviderLocationDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Location payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var location = mapper.Map<ProviderLocations>(dto);
            location.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<ProviderLocations>().AddAsync(location);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderLocationDto>(location);
        }

        public async Task<GetProviderLocationDto> UpdateLocationAsync(string userId, Guid locationId, UpdateProviderLocationDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Location payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var location = await unitOfWork.Repository<ProviderLocations>().GetByIdAsync(locationId);
            if (location == null || location.ProviderProfileId != profile.Id)
                throw new NotFoundException("Location not found.");

            location.BranchName = dto.BranchName;
            location.Address = dto.Address;
            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;
            location.PostalCode = dto.PostalCode;
            location.ServicePhoneNumber = dto.ServicePhoneNumber;
            location.IsActive = dto.IsActive;

            unitOfWork.Repository<ProviderLocations>().Update(location);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderLocationDto>(location);
        }

        public async Task DeleteLocationAsync(string userId, Guid locationId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var location = await unitOfWork.Repository<ProviderLocations>().GetByIdAsync(locationId);
            if (location == null || location.ProviderProfileId != profile.Id)
                throw new NotFoundException("Location not found.");

            unitOfWork.Repository<ProviderLocations>().Delete(location);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<GetProviderDocumentDto>> GetMyDocumentsAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var files = await unitOfWork.Repository<StoredFile>()
                .GetAllAsync(new StoredFilesByProviderSpecification(profile.Id));
            return mapper.Map<IReadOnlyList<GetProviderDocumentDto>>(files);
        }

        public async Task<GetProviderDocumentDto> UploadDocumentAsync(string userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("License file is required.");

            if (file.Length > 5 * 1024 * 1024)
                throw new BadRequestException("License file must not exceed 5 MB.");

            if (string.IsNullOrWhiteSpace(file.FileName))
                throw new BadRequestException("License file name is required.");

            var extension = System.IO.Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
                throw new BadRequestException("License file must be PDF, JPG or PNG.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var path = await fileService.UploadFileAsync(file, "ProviderDocuments");
            var storedFile = new StoredFile
            {
                FileName = file.FileName,
                FilePath = path,
                FileType = file.ContentType,
                ProviderProfileId = profile.Id
            };
            await unitOfWork.Repository<StoredFile>().AddAsync(storedFile);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderDocumentDto>(storedFile);
        }

        public async Task DeleteDocumentAsync(string userId, Guid documentId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var file = await unitOfWork.Repository<StoredFile>().GetByIdAsync(documentId);
            if (file == null || file.ProviderProfileId != profile.Id)
                throw new NotFoundException("Document not found.");

            await fileService.DeleteAsync(file.FilePath);
            unitOfWork.Repository<StoredFile>().Delete(file);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<GetProviderServiceDto>> GetMyServicesAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var services = await unitOfWork.Repository<ProviderService>()
                .GetAllAsync(new ProviderServicesByProviderSpecification(profile.Id));
            return mapper.Map<IReadOnlyList<GetProviderServiceDto>>(services);
        }

        public async Task<GetProviderServiceDto> CreateServiceAsync(string userId, CreateProviderServiceDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Service payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var service = mapper.Map<ProviderService>(dto);
            service.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<ProviderService>().AddAsync(service);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderServiceDto>(service);
        }

        public async Task<GetProviderServiceDto> UpdateServiceAsync(string userId, Guid serviceId, UpdateProviderServiceDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Service payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            var service = await unitOfWork.Repository<ProviderService>().GetByIdAsync(serviceId);
            if (service == null || service.ProviderProfileId != profile.Id)
                throw new NotFoundException("Service not found.");

            service.ServiceName = dto.ServiceName;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.IsActive = dto.IsActive;

            unitOfWork.Repository<ProviderService>().Update(service);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderServiceDto>(service);
        }

        public async Task DeleteServiceAsync(string userId, Guid serviceId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var service = await unitOfWork.Repository<ProviderService>().GetByIdAsync(serviceId);
            if (service == null || service.ProviderProfileId != profile.Id)
                throw new NotFoundException("Service not found.");

            unitOfWork.Repository<ProviderService>().Delete(service);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<MedicalRecordDto> CreateMedicalRecordAsync(string providerUserId, CreateMedicalRecordDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Medical record payload is required.");

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Title is required.");
            if (dto.Title.Length > 200)
                throw new BadRequestException("Title cannot exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new BadRequestException("Description is required.");
            if (dto.Description.Length > 500)
                throw new BadRequestException("Description cannot exceed 500 characters.");

            if (string.IsNullOrWhiteSpace(dto.Diagnosis))
                throw new BadRequestException("Diagnosis is required.");

            var profile = await GetProviderProfileByUserIdAsync(providerUserId);
            if (profile.verificationStatus != VerificationStatus.Approved)
                throw new ForbiddenException("Provider is not approved.");

            var patient = await userManager.FindByIdAsync(dto.PatientId);
            if (patient == null)
                throw new NotFoundException("Patient not found.");

            if (!await userManager.IsInRoleAsync(patient, Roles.User))
                throw new ForbiddenException("Patient access is required.");

            var record = mapper.Map<MedicalRecord>(dto);
            record.PatientId = dto.PatientId;
            record.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<MedicalRecord>().AddAsync(record);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<MedicalRecordDto>(record);
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetMyMedicalRecordsAsync(string patientUserId)
        {
            var user = await userManager.FindByIdAsync(patientUserId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (!await userManager.IsInRoleAsync(user, Roles.User))
                throw new ForbiddenException("Patient access is required.");

            var records = await unitOfWork.Repository<MedicalRecord>()
                .GetAllAsync(new MedicalRecordsByPatientSpecification(patientUserId));
            return mapper.Map<IReadOnlyList<MedicalRecordDto>>(records);
        }

        public async Task<MedicalRecordDto> GetMedicalRecordAsync(Guid recordId, string userId)
        {
            var record = await unitOfWork.Repository<MedicalRecord>().GetByIdAsync(recordId);
            if (record == null)
                throw new NotFoundException("Medical record not found.");

            var isPatient = record.PatientId == userId;

            var providerProfile = await unitOfWork.Repository<ProviderProfile>()
                .FindAsync(new ProviderProfileByUserSpecification(userId));
            var isProvider = providerProfile != null && providerProfile.Id == record.ProviderProfileId;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (!await userManager.IsInRoleAsync(user, Roles.User) &&
                !await userManager.IsInRoleAsync(user, Roles.Provider))
                throw new ForbiddenException("You are not allowed to access this medical record.");

            if (!isPatient && !isProvider)
                throw new ForbiddenException("You are not allowed to access this medical record.");

            return mapper.Map<MedicalRecordDto>(record);
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetMyCreatedMedicalRecordsAsync(string providerUserId)
        {
            var profile = await GetProviderProfileByUserIdAsync(providerUserId);
            var records = await unitOfWork.Repository<MedicalRecord>()
                .GetAllAsync(new MedicalRecordsByProviderSpecification(profile.Id));
            return mapper.Map<IReadOnlyList<MedicalRecordDto>>(records);
        }

        public async Task<IReadOnlyList<GetProviderServiceDto>> GetPublicServicesAsync(Guid providerProfileId)
        {
            var services = await unitOfWork.Repository<ProviderService>()
                .GetAllAsync(new ProviderServicesByProviderSpecification(providerProfileId));
            return mapper.Map<IReadOnlyList<GetProviderServiceDto>>(
                services.Where(s => s.IsActive).ToList());
        }

        public async Task<ReviewDto> CreateReviewAsync(string patientUserId, CreateReviewDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Review payload is required.");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new BadRequestException("Rating must be between 1 and 5.");

            var patient = await userManager.FindByIdAsync(patientUserId);
            if (patient == null)
                throw new NotFoundException("Patient not found.");

            if (!await userManager.IsInRoleAsync(patient, Roles.User))
                throw new ForbiddenException("Patient access is required.");

            var providerProfile = await unitOfWork.Repository<ProviderProfile>().GetByIdAsync(dto.ProviderProfileId);
            if (providerProfile == null)
                throw new NotFoundException("Provider profile not found.");

            var review = mapper.Map<Review>(dto);
            review.PatientId = patientUserId;
            review.ProviderProfileId = dto.ProviderProfileId;
            await unitOfWork.Repository<Review>().AddAsync(review);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<ReviewDto>(review);
        }

        public async Task<IReadOnlyList<ReviewDto>> GetProviderReviewsAsync(Guid providerProfileId)
        {
            var reviews = await unitOfWork.Repository<Review>()
                .GetAllAsync(new ReviewsByProviderSpecification(providerProfileId));
            return mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }

        public async Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(string patientUserId)
        {
            var reviews = await unitOfWork.Repository<Review>()
                .GetAllAsync(new ReviewsByPatientSpecification(patientUserId));
            return mapper.Map<IReadOnlyList<ReviewDto>>(reviews);
        }

        public async Task<IReadOnlyList<ProviderAvailabilityDto>> GetMyAvailabilityAsync(string userId)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var items = await unitOfWork.Repository<ProviderAvailability>()
                .GetAllAsync(new ProviderAvailabilitiesByProviderSpecification(profile.Id));
            return mapper.Map<IReadOnlyList<ProviderAvailabilityDto>>(items);
        }

        public async Task SetMyAvailabilityAsync(string userId, SetProviderAvailabilityDto dto)
        {
            if (dto == null)
                throw new BadRequestException("Availability payload is required.");

            var profile = await GetProviderProfileByUserIdAsync(userId);
            if (profile.verificationStatus != VerificationStatus.Approved)
                throw new ForbiddenException("Provider is not approved.");

            var intervals = dto.Intervals ?? new List<ProviderAvailabilityDto>();

            foreach (var interval in intervals)
            {
                if (interval.StartTime >= interval.EndTime)
                    throw new BadRequestException("Start time must be earlier than end time.");

                if (!Enum.IsDefined(typeof(DayOfWeek), interval.DayOfWeek))
                    throw new BadRequestException("Invalid day of week.");
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

            var existing = await unitOfWork.Repository<ProviderAvailability>()
                .GetAllAsync(new ProviderAvailabilitiesByProviderSpecification(profile.Id));

            await unitOfWork.BeginTransactionAsync();
            try
            {
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

                profile.HasAvailabilityConfigured = true;
            unitOfWork.Repository<ProviderProfile>().Update(profile);

            await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}