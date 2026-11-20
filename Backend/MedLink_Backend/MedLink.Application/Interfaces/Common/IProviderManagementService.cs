using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface IProviderManagementService
    {
        Task<GetProviderProfileDto> GetMyProfileAsync(string userId);
        Task<GetProviderProfileDto> UpdateMyProfileAsync(string userId, UpdateProviderProfileDto dto);

        Task<IReadOnlyList<GetProviderLocationDto>> GetMyLocationsAsync(string userId);
        Task<GetProviderLocationDto> CreateLocationAsync(string userId, CreateProviderLocationDto dto);
        Task<GetProviderLocationDto> UpdateLocationAsync(string userId, Guid locationId, UpdateProviderLocationDto dto);
        Task DeleteLocationAsync(string userId, Guid locationId);

        Task<IReadOnlyList<GetProviderDocumentDto>> GetMyDocumentsAsync(string userId);
        Task<GetProviderDocumentDto> UploadDocumentAsync(string userId, Microsoft.AspNetCore.Http.IFormFile file);
        Task DeleteDocumentAsync(string userId, Guid documentId);

        Task<IReadOnlyList<GetProviderServiceDto>> GetMyServicesAsync(string userId);
        Task<GetProviderServiceDto> CreateServiceAsync(string userId, CreateProviderServiceDto dto);
        Task<GetProviderServiceDto> UpdateServiceAsync(string userId, Guid serviceId, UpdateProviderServiceDto dto);
        Task DeleteServiceAsync(string userId, Guid serviceId);

        Task<MedicalRecordDto> CreateMedicalRecordAsync(string providerUserId, CreateMedicalRecordDto dto);
        Task<MedicalRecordDto> GetMedicalRecordAsync(Guid recordId, string userId);
        Task<IReadOnlyList<MedicalRecordDto>> GetMyMedicalRecordsAsync(string patientUserId);
        Task<IReadOnlyList<MedicalRecordDto>> GetMyCreatedMedicalRecordsAsync(string providerUserId);

        Task<IReadOnlyList<GetProviderServiceDto>> GetPublicServicesAsync(Guid providerProfileId);

        Task<ReviewDto> CreateReviewAsync(string patientUserId, CreateReviewDto dto);
        Task<IReadOnlyList<ReviewDto>> GetProviderReviewsAsync(Guid providerProfileId);
        Task<IReadOnlyList<ReviewDto>> GetMyReviewsAsync(string patientUserId);

        Task<IReadOnlyList<ProviderAvailabilityDto>> GetMyAvailabilityAsync(string userId);
        Task SetMyAvailabilityAsync(string userId, SetProviderAvailabilityDto dto);
    }
}