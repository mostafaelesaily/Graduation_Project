using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class GetProviderRequestDto
    {
        public Guid ProviderId { get; set; }
        public string ServiceName { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        public string? LicenseImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public MedLink.Domain.Enums.VerificationStatus Status { get; set; }
        
    }
}
