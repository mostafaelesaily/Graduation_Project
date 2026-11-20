using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class UpdateProviderProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string ServiceName { get; set; } = string.Empty;
    }
}