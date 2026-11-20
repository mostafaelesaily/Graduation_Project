using MedLink.Domain.Entities.Common.Identity;
using System;

namespace MedLink.Domain.Entities.Common
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string PatientId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }

        public AppUser Patient { get; set; } = null!;
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}