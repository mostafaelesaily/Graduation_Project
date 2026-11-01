using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Enums;
using System;

namespace MedLink.Domain.Entities.Common
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public AppUser User { get; set; } = null!;
        public ProviderProfile ProviderProfile { get; set; } = null!;
    }
}
