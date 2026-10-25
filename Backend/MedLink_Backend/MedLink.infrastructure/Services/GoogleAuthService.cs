using Google.Apis.Auth;
using MedLink.Application.DTOs.Common.identityDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedLink.infrastructure.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        public GoogleAuthService(IConfiguration configuration, ILogger<GoogleAuthService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        private readonly IConfiguration configuration;
        private readonly ILogger<GoogleAuthService> logger;

        public async Task<GoogleUserInfoDto> ValidateIdTokenAsync(string idToken)
        {
            var clientId = configuration["GoogleAuth:WEB_CLIENT_ID"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                logger.LogError("Google sign-in is not configured. Missing GoogleAuth:WEB_CLIENT_ID.");
                throw new BadRequestException("Google sign-in is not configured on the server.");
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            }
            catch (InvalidJwtException ex)
            {
                logger.LogWarning(ex, "Google ID token validation failed.");
                throw new UnauthorizedException("Invalid or expired Google token.");
            }

            if (!payload.EmailVerified)
            {
                logger.LogWarning("Google sign-in rejected because the Google account email is not verified. Subject: {Subject}", payload.Subject);
                throw new UnauthorizedException("Google account email is not verified.");
            }

            return new GoogleUserInfoDto
            {
                Subject = payload.Subject,
                Email = payload.Email,
                Name = payload.Name
            };
        }
    }
}
