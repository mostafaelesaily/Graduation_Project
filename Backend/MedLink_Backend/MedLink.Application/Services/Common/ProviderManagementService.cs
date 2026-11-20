using AutoMapper;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
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
            var profiles = await unitOfWork.Repository<ProviderProfile>().GetAllAsync();
            var profile = profiles.FirstOrDefault(p => p.userId == userId);
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
            var locations = await unitOfWork.Repository<ProviderLocations>().GetAllAsync();
            return mapper.Map<IReadOnlyList<GetProviderLocationDto>>(
                locations.Where(l => l.ProviderProfileId == profile.Id).ToList());
        }

        public async Task<GetProviderLocationDto> CreateLocationAsync(string userId, CreateProviderLocationDto dto)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var location = mapper.Map<ProviderLocations>(dto);
            location.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<ProviderLocations>().AddAsync(location);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderLocationDto>(location);
        }

        public async Task<GetProviderLocationDto> UpdateLocationAsync(string userId, Guid locationId, UpdateProviderLocationDto dto)
        {
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
            var files = await unitOfWork.Repository<StoredFile>().GetAllAsync();
            return mapper.Map<IReadOnlyList<GetProviderDocumentDto>>(
                files.Where(f => f.ProviderProfileId == profile.Id).ToList());
        }

        public async Task<GetProviderDocumentDto> UploadDocumentAsync(string userId, IFormFile file)
        {
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
            var services = await unitOfWork.Repository<ProviderService>().GetAllAsync();
            return mapper.Map<IReadOnlyList<GetProviderServiceDto>>(
                services.Where(s => s.ProviderProfileId == profile.Id).ToList());
        }

        public async Task<GetProviderServiceDto> CreateServiceAsync(string userId, CreateProviderServiceDto dto)
        {
            var profile = await GetProviderProfileByUserIdAsync(userId);
            var service = mapper.Map<ProviderService>(dto);
            service.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<ProviderService>().AddAsync(service);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<GetProviderServiceDto>(service);
        }

        public async Task<GetProviderServiceDto> UpdateServiceAsync(string userId, Guid serviceId, UpdateProviderServiceDto dto)
        {
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
            var profile = await GetProviderProfileByUserIdAsync(providerUserId);
            var patient = await userManager.FindByIdAsync(dto.PatientId);
            if (patient == null)
                throw new NotFoundException("Patient not found.");

            var record = mapper.Map<MedicalRecord>(dto);
            record.PatientId = dto.PatientId;
            record.ProviderProfileId = profile.Id;
            await unitOfWork.Repository<MedicalRecord>().AddAsync(record);
            await unitOfWork.SaveChangesAsync();
            return mapper.Map<MedicalRecordDto>(record);
        }

        public async Task<IReadOnlyList<MedicalRecordDto>> GetMyMedicalRecordsAsync(string patientUserId)
        {
            var records = await unitOfWork.Repository<MedicalRecord>().GetAllAsync();
            return mapper.Map<IReadOnlyList<MedicalRecordDto>>(
                records.Where(r => r.PatientId == patientUserId).ToList());
        }
    }
}