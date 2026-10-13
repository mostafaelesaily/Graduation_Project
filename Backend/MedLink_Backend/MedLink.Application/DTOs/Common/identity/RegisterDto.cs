using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.identity
{
    public class RegisterDto
    {
        public string userName { get; set; }
        public string Email { get; set; }
        public string phoneNumber { get; set; }
        public string Password { get; set; }
    }
}
