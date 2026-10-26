using MedLink.Domain.Entities.Common.Notifications;

namespace MedLink.Application.DTOs.Common.NotificationDTOs
{
    public class CreateAnnouncementDto
    {
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        public NotificationAudience TargetAudience { get; set; }
    }
}