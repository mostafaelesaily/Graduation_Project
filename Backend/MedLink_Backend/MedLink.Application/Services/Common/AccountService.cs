using AutoMapper;
using MedLink.Application.DTOs.Common.identity;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;


namespace MedLink.Application.Services.Common
{
    public class AccountService : IAccountService
    {
        public AccountService
        (
            IMapper mapper,
            IConfiguration configuration,
            ILogger<AccountService> logger,
            UserManager<AppUser> userManager,
            IMailService mailService
        )
        {
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
            this.logger = logger;
            this.mailService = mailService;
        }
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ILogger<AccountService> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly IMailService mailService;
        public async Task<string> GenrateAccessToken(AppUser appUser)
        {
            var claims = new List<Claim>()
            {
             new Claim(ClaimTypes.NameIdentifier, appUser.Id),
             new Claim(ClaimTypes.Email, appUser.Email),
             new Claim(ClaimTypes.Name, appUser.UserName),
             new Claim(JwtRegisteredClaimNames.Jti , Guid.NewGuid().ToString())
            };
            var roles = await userManager.GetRolesAsync(appUser);
            foreach(var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var sigKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: sigKey
            );
            var _token = new JwtSecurityTokenHandler().WriteToken(token);
            return _token;
        }

        public async Task<RefreshTokens> GenrateRefreshToken()
        {
            return new RefreshTokens
            {
                CreatedOn = DateTime.UtcNow,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            };
        }

        public async Task<AuthenticationResponseDto> HandleRefreshTokenAsync(GenrateRefreshToken genrateRefreshTokenDto)
        {
            if (genrateRefreshTokenDto == null ) throw new ArgumentNullException(nameof(genrateRefreshTokenDto));
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == genrateRefreshTokenDto.Token));
            if (user == null) throw new UnauthorizedException("Invalid token");
            var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == genrateRefreshTokenDto.Token);
            if (refreshToken == null || refreshToken.ExpiresOn < DateTime.UtcNow || refreshToken.revokedOn != null)
            { throw new UnauthorizedException("Invalid token"); }
            refreshToken.revokedOn = DateTime.UtcNow;
            var newAccessToken = await GenrateAccessToken(user);
            var newRefreshToken = await GenrateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await userManager.UpdateAsync(user);
            return new AuthenticationResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpireOn = DateTime.UtcNow.AddHours(3),
                Message = "Token refreshed successfully",
            };
        }

        public async Task<AuthenticationResponseDto> Login(LoginDto loginDto)
        {
            if (loginDto == null) throw new ArgumentNullException(nameof(loginDto));
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            logger.LogInformation("Attempting login for user with email: {Email}", loginDto.Email);

            if (user == null) throw new NotFoundException($"User with email {loginDto.Email} not found.");
            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                logger.LogWarning("Invalid password attempt for user with email: {Email}", loginDto.Email);
                throw new UnauthorizedException("Invalid Email Or Password");
            }
            if (!user.EmailConfirmed)
            {
                logger.LogWarning("Login attempt failed for user with email: {Email}. Email not confirmed.", loginDto.Email);
                throw new UnauthorizedException("Email not confirmed. Please confirm your email before logging in.");
            }
            var accessToken = await GenrateAccessToken(user);
            var refreshToken = await GenrateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await userManager.UpdateAsync(user);
            return new AuthenticationResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpireOn = DateTime.UtcNow.AddHours(3),
                Message = "Login successful"
            };
        }

        public async Task<RegisterResponseDto> Register(RegisterDto appUser)
        {
            logger.LogInformation("Registering new user with email: {Email}", appUser.Email);
            if (appUser == null) throw new ArgumentNullException(nameof(appUser));
            var user = mapper.Map<AppUser>(appUser);
            var result = await userManager.CreateAsync(user, appUser.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning("User registration failed for email: {Email}. Errors: {Errors}",
                appUser.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                var errors = string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new  BadRequestException("User registration failed. Errors: " + errors);
            }
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = $"https://localhost:44302/api/Account/ConfirmMail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
            try
            {
                await mailService.sendMail
                    (user.Email, "Confirm your email", $"Please confirm your account by clicking this link: <a href='{confirmationLink}'>Confirm Email</a>");
                logger.LogInformation("Confirmation email sent successfully to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email to: {Email}. Reason: {Reason}", user.Email, ex.Message);
            }

            logger.LogInformation("User registered successfully with email: {Email}", appUser.Email);
            return new RegisterResponseDto
            {
                message = "User Registered Successfully"
            };
        }

        public async Task<string> ConfirmMail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
           
            if (user == null) 
            {
                logger.LogWarning("User not found for email confirmation with userId: {UserId}", userId);
                throw new NotFoundException("User Not Found"); 
            }
            var Result = await userManager.ConfirmEmailAsync(user, token);
            if (!Result.Succeeded)
            {
                var errors = string.Join(", ",
                    Result.Errors.Select(e => e.Description));
                throw new BadRequestException("Email confirmation failed. Errors: " + errors);
            }
            return "Email confirmed successfully."; 
        }

        public async Task Logout(string userId)
        {
          logger.LogInformation("Logging out user with userId: {UserId}", userId);
            var user = userManager.Users.Include(u => u.RefreshTokens)
                .FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                logger.LogWarning("User not found for logout with userId: {UserId}", userId);
                throw new NotFoundException("User Not Found");
            }
            foreach (var refreshToken in user.RefreshTokens)
            {
                refreshToken.revokedOn = DateTime.UtcNow;
            }
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to update user with userId: {UserId}", userId);
                throw new Exception("Failed to logout user");
            }
            logger.LogInformation("User with ID: {UserId} logged out successfully.", userId);
        }
    }
}
