using System;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class ReviewDto
    {
        public Guid Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PatientId { get; set; } = string.Empty;
        public Guid ProviderProfileId { get; set; }
    }

    public class CreateReviewDto
    {
        [Required]
        public Guid ProviderProfileId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? Comment { get; set; }
    }
}