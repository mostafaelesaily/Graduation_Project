using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class EditAvailabilityDto
    {
        [Required]
        public List<GetAvailabilityDto> Intervals { get; set; } = new();
    }
}