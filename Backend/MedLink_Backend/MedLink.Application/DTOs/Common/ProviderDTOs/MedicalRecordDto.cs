using System;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class MedicalRecordDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }
    }
}