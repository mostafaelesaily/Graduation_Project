using System;

namespace MedLink.Domain.Entities.Common
{
    public class ProviderAvailability
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProviderProfileId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}