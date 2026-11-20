using System;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class ProviderAvailabilityDto
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}