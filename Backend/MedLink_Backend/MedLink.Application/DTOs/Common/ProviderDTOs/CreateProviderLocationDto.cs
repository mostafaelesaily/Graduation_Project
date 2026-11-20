using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class CreateProviderLocationDto
    {
        [MaxLength(100)]
        public string? BranchName { get; set; }

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ServicePhoneNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}