using MedLink.Domain.Entities.Common.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Domain.Entities.Common
{
    public class Review
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PatientId { get; set; }
        public string ProviderId { get; set; }
        public AppUser Provider { get; set; }
        public AppUser Patient { get; set; }
    }
}
