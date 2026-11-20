using System;

namespace MedLink.Domain.Entities.Common
{
    public class ProviderService
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid ProviderProfileId { get; set; }
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}