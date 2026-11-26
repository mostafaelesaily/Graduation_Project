using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.BookingDTOs
{
    public class UpdateBookingDto
    {
        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
