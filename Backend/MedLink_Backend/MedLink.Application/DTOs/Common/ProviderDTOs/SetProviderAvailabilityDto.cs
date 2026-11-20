using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ProviderDTOs
{
    public class SetProviderAvailabilityDto
    {
        [Required]
        public List<ProviderAvailabilityDto> Intervals { get; set; } = new();
    }
}