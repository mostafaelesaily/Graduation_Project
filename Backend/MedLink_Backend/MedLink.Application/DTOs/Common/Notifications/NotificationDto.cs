using System;
using System.Collections.Generic;
using System.Text;


namespace MedLink.Application.DTOs.Common.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? ReadAtUtc { get; set; }

        public string? Link { get; set; }
    }
}

