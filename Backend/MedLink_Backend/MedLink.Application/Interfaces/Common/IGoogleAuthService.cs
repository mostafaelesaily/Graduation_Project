using MedLink.Application.DTOs.Common.identityDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IGoogleAuthService
    {
        Task<GoogleUserInfoDto> ValidateIdTokenAsync(string idToken);
    }
}
