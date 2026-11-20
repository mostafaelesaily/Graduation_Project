using System;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class GetProviderServiceDto
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}