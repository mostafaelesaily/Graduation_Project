using System;

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
        public bool HasAvailabilityConfigured { get; set; }
    }
}