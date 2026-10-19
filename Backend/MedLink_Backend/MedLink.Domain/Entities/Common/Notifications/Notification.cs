using System;

namespace MedLink.Domain.Entities.Common.Notifications
{
    public class Notification
    {
        // Single source of truth for the limits
        public const int TitleMaxLength = 150;
        public const int MessageMaxLength = 1000;
        public const int TypeMaxLength = 50;
        public const int LinkMaxLength = 500;

        public Guid Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAtUtc { get; set; }

        public string? Link { get; set; }
    }
}