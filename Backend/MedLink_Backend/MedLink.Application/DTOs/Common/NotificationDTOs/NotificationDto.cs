using MedLink.Domain.Entities.Common.Notifications;

namespace MedLink.Application.DTOs.Common.NotificationDTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }

        public Guid NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string? Link { get; set; }

        public NotificationSource Source { get; set; }

        public NotificationAudience? TargetAudience { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAtUtc { get; set; }

        public bool IsHidden { get; set; }
    }
}