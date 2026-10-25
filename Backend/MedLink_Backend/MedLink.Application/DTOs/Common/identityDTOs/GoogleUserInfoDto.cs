using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.identityDTOs
{
    public class GoogleUserInfoDto
    {
        public string Subject { get; set; }
        public string Email { get; set; }
        public string? Name { get; set; }
    }
}
