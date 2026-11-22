using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Provider
{
    public class ProviderService
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid ProviderProfileId { get; set; }
        public ProviderProfile ProviderProfile { get; set; }
    }
}
