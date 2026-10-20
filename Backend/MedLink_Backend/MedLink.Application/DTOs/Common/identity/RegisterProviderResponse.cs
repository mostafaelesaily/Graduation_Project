using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.DTOs.Common.identity
{
    public class RegisterProviderResponse : RegisterResponseDto
    {
        public string Email { get; set; }
        public VerificationStatus ApprovalStatus { get; set; }
    }
}
