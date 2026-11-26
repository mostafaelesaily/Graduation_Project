using MedLink.Domain.Entities.Common.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common
{
    public class Notifications
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public AppUser User { get; set; }

    }
}
