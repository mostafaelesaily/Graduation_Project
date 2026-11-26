using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Entities.Medical_History;

namespace MedLink.Domain.Entities.Common.Identity
{
    public class AppUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
        public ICollection<RefreshTokens> RefreshTokens { get; set; }
        public ICollection<Notifications> Notifications { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
        public ICollection<Review> PatientReviews { get; set; } = new List<Review>();
        public ICollection<Review> ProviderReviews { get; set; } = new List<Review>();
        public ProviderProfile providerProfile { get; set; }
    }
}
