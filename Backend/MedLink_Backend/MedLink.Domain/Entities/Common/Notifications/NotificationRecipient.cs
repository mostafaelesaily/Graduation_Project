using System;

namespace MedLink.Domain.Entities.Common.Notifications
{
    public class NotificationRecipient
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid NotificationId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAtUtc { get; set; }

        public bool IsHidden { get; set; } = false;

        public DateTime? HiddenAtUtc { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Notification Notification { get; set; } = null!;
    }
}