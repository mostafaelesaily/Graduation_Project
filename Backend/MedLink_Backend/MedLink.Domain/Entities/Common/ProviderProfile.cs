using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common
{
    public class ProviderProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string userId { get; set; }
        public string ProviderName { get; set; }
        public ProviderType ProviderType { get; set; }
        public string TaxNumber { get; set; }
        public Guid LicenseFileId { get; set; }
        public VerificationStatus verificationStatus { get; set; } = VerificationStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public AppUser user { get; set; }
        public ICollection<ProviderLocations> ProviderLocations { get; set; } = new List<ProviderLocations>();

    }
}
