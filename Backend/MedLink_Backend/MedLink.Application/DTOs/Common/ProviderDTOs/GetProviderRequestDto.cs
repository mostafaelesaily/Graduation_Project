using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class GetProviderRequestDto
    {
        public Guid ProviderId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? LicenseImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}
