using MedLink.Application.DTOs.Common.identity;
using MedLink.Application.DTOs.Common.identityDTOs;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface IAccountService
    {
        public Task <string> GenrateAccessToken(AppUser appUser);
        public Task<RefreshTokens> GenrateRefreshToken();
        public Task<AuthenticationResponseDto> HandleRefreshTokenAsync(GenrateRefreshToken genrateRefreshTokenDto);
        public Task<RegisterResponseDto> Register(RegisterDto appUser);
        Task<RegisterProviderResponse> RegisterProvider(RegisterProviderRequest providerRequest);
        public Task<AuthenticationResponseDto> Login(LoginDto loginDto);
        public Task<string> ConfirmMail(string userId, string token);
        Task<AuthenticationResponseDto> GoogleSignInAsync(GoogleLoginDto request);
        public Task Logout(string userId);

    }
}
