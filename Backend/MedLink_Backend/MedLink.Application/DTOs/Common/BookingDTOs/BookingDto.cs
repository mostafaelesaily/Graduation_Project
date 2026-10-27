using MedLink.Domain.Enums;
using System;

namespace MedLink.Application.DTOs.Common.BookingDTOs
{
    public class BookingDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }
        public DateTime BookingDate { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
