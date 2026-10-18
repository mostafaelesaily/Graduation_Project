using System;
using System.Collections.Generic;
using System.Text;


namespace MedLink.Domain.Entities.Common.Notifications
{
    public class Notification
    {
        public int Id { get; set; }

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

