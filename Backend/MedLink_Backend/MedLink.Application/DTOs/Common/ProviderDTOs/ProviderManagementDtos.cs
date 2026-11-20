using System;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class GetProviderProfileDto
    {
        public Guid ProviderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string VerificationStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateProviderProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ServiceName { get; set; } = string.Empty;
    }

    public class GetProviderLocationDto
    {
        public Guid Id { get; set; }
        public string? BranchName { get; set; }
        public string Address { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        public string ServicePhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class CreateProviderLocationDto
    {
        [MaxLength(100)]
        public string? BranchName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ServicePhoneNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProviderLocationDto : CreateProviderLocationDto { }

    public class GetProviderDocumentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GetProviderServiceDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProviderServiceDto
    {
        [Required]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateProviderServiceDto : CreateProviderServiceDto { }

    public class MedicalRecordDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string? Description { get; set; }
        public string? Diagnosis { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }
    }

    public class CreateMedicalRecordDto
    {
        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime RecordDate { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(2000)]
        public string? Diagnosis { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}