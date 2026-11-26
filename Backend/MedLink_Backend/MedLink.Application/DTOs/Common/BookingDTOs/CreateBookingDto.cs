using System;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.BookingDTOs
{
    public class CreateBookingDto
    {
        [Required]
        public Guid ProviderProfileId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }
    }
}
