using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.BookingDTOs
{
    public class CancelBookingDto
    {
        [MaxLength(500)]
        public string? CancellationReason { get; set; }
    }

    public class BulkCancelBookingsDto
    {
        [Required]
        public List<Guid> BookingIds { get; set; } = new();

        [MaxLength(500)]
        public string? CancellationReason { get; set; }
    }

    public class BulkCancelBookingsResultDto
    {
        public List<Guid> CancelledBookingIds { get; set; } = new();
        public List<Guid> RejectedBookingIds { get; set; } = new();
    }

    public class ProviderBookingStatisticsDto
    {
        public int TodayCount { get; set; }
        public int NextSevenDaysCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
    }
}