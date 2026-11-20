using MedLink.Domain.Entities.Common.Identity;
using System;

namespace MedLink.Domain.Entities.Common
{
    public class MedicalRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public DateTime RecordDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string PatientId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }

        public AppUser Patient { get; set; } = null!;
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}