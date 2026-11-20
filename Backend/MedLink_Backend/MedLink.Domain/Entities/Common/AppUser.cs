using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using MedLink.Domain.Entities.Common;

namespace MedLink.Domain.Entities.Common.Identity
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsActice { get; set; } = true;
        public ICollection<RefreshTokens> RefreshTokens { get; set; }
        public ICollection<Notifications> notifications { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ProviderProfile providerProfile { get; set; }
    }
}
