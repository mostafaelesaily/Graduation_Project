using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class GetProviderRequestDto
    {
        public Guid ProviderId { get; set; }
        public string ServiceName { get; set; }
        public string PhoneNumber { get; set; }
        public string? LicenseImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        
    }
}
