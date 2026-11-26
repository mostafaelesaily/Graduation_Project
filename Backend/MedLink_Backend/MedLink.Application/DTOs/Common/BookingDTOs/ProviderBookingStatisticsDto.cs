namespace MedLink.Application.DTOs.Common.BookingDTOs
{
    public class ProviderBookingStatisticsDto
    {
        public int TodayCount { get; set; }
        public int NextSevenDaysCount { get; set; }
        public int PendingCount { get; set; }
        public int CancelledCount { get; set; }
    }
}