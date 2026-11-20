using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class CreateProviderServiceDto
    {
        [Required]
        [MaxLength(200)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;
    }
}