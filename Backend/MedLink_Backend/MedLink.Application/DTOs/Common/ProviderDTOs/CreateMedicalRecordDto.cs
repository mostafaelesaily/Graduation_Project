using System;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class CreateMedicalRecordDto
    {
        [Required]
        public string PatientId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public DateTime RecordDate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Diagnosis { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}