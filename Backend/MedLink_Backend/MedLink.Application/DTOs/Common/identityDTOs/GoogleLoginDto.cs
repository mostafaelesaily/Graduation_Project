using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MedLink.Application.DTOs.Common.identityDTOs
{
    public class GoogleLoginDto
    {
        [Required]
        public string idToken { get; set; }
    }
}
