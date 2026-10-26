using System;

namespace MedLink.Domain.Entities.Common.Notifications
{
    public enum NotificationSource
    {
        System = 0,
        Admin = 1
    }

    public enum NotificationAudience
    {
        User = 0,
        Provider = 1,
        Both = 2
    }

    public class Notification
    {
        public const int TitleMaxLength = 150;
        public const int MessageMaxLength = 1000;
        public const int TypeMaxLength = 50;
        public const int LinkMaxLength = 500;

        public Guid Id { get; set; } = Guid.NewGuid();

        public NotificationSource Source { get; set; } = NotificationSource.System;

        public string Type { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Link { get; set; }

        public NotificationAudience? TargetAudience { get; set; }

        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }
}