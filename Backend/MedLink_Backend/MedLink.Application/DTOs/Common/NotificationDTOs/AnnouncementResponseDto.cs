using MedLink.Domain.Entities.Common.Notifications;

namespace MedLink.Application.DTOs.Common.NotificationDTOs
{
    public class AnnouncementResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        public NotificationAudience? TargetAudience { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public int RecipientCount { get; set; }
    }
}