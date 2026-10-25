using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.identity
{
    public class RegisterProviderRequest : RegisterDto
    {
        public string ProviderName { get; set; }   // ← جديد

        public string Address { get; set; }
        public string postalCode { get; set; }
        public string taxNumber { get; set; }
        public IFormFile LicenseFile { get; set; }
        public ProviderType ProviderType { get; set; }

    }
}
