using AutoMapper;
using MedLink.Application.Constants;
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
using MedLink.Domain.Enums;

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
            IMailService mailService,
            IFileService fileService,
            IUnitOfWork unitOfWork
        )
        {
            this.mapper = mapper;
            this.configuration = configuration;
            this.userManager = userManager;
            this.logger = logger;
            this.mailService = mailService;
            this.fileService = fileService;
            this.unitOfWork = unitOfWork;

        }

    
        private readonly IMapper mapper;
        private readonly IConfiguration configuration;
        private readonly ILogger<AccountService> logger;
        private readonly UserManager<AppUser> userManager;
        private readonly IMailService mailService;
        private readonly IFileService fileService;

        private readonly IUnitOfWork unitOfWork;

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
            if (!user.IsActice)
            {
                logger.LogWarning("Login attempt failed for user with email: {Email}. Account is not active.", loginDto.Email);
                throw new UnauthorizedException("Your account is not active yet. Please wait for admin approval.");
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
            var confirmationLink = $"{configuration["AppUrl"]}/api/Account/ConfirmMail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
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

        public async Task<RegisterProviderResponse> RegisterProvider(RegisterProviderRequest providerRequest)
        {
            if (providerRequest == null) throw new ArgumentNullException(nameof(providerRequest));

            logger.LogInformation("Registering new provider with email: {Email}", providerRequest.Email);

            ValidateProviderRequest(providerRequest);

            var existingEmailUser = await userManager.FindByEmailAsync(providerRequest.Email);
            if (existingEmailUser != null)
            {
                logger.LogWarning("Provider registration failed. Email already in use: {Email}", providerRequest.Email);
                throw new ConflictException($"Email {providerRequest.Email} is already in use.");
            }

            var existingUserNameUser = await userManager.FindByNameAsync(providerRequest.userName);
            if (existingUserNameUser != null)
            {
                logger.LogWarning("Provider registration failed. Username already taken: {UserName}", providerRequest.userName);
                throw new ConflictException($"Username {providerRequest.userName} is already taken.");
            }

            await unitOfWork.BeginTransactionAsync();
            try
            {
                var user = mapper.Map<AppUser>(providerRequest);
                var createResult = await userManager.CreateAsync(user, providerRequest.Password);
                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Provider registration failed for email: {Email}. Errors: {Errors}",
                        providerRequest.Email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new BadRequestException("Provider registration failed. Errors: " + errors);
                }

                var roleResult = await userManager.AddToRoleAsync(user, Roles.Provider);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new BadRequestException("Failed to assign provider role. Errors: " + errors);
                }

                var licenseFilePath = await fileService.UploadFileAsync(providerRequest.LicenseFile, "ProviderLicenses");
                var storedFile = new StoredFile
                {
                    FileName = providerRequest.LicenseFile.FileName,
                    FilePath = licenseFilePath,
                    FileType = providerRequest.LicenseFile.ContentType
                };
                await unitOfWork.Repository<StoredFile>().AddAsync(storedFile);

                var providerProfile = mapper.Map<ProviderProfile>(providerRequest);
                providerProfile.userId = user.Id;
                providerProfile.LicenseFileId = storedFile.Id;
                await unitOfWork.Repository<ProviderProfile>().AddAsync(providerProfile);

                var providerLocation = mapper.Map<ProviderLocations>(providerRequest);
                providerLocation.ProviderProfileId = providerProfile.Id;
                providerLocation.IsActive = true;
                await unitOfWork.Repository<ProviderLocations>().AddAsync(providerLocation);

                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = $"{configuration["AppUrl"]}/api/Account/ConfirmMail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
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

                await unitOfWork.CommitTransactionAsync();

                logger.LogInformation("Provider registered successfully with email: {Email}", providerRequest.Email);

                return new RegisterProviderResponse
                {
                    message = "Provider registration submitted successfully. Your account will be reviewed by an admin after email confirmation.",
                    Email = user.Email,
                    ApprovalStatus = providerProfile.verificationStatus
                };
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private static void ValidateProviderRequest(RegisterProviderRequest providerRequest)
        {
            if (string.IsNullOrWhiteSpace(providerRequest.Email))
                throw new BadRequestException("Email is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.userName))
                throw new BadRequestException("Username is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.Password))
                throw new BadRequestException("Password is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.phoneNumber))
                throw new BadRequestException("Phone number is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.ProviderName))
                throw new BadRequestException("Provider name is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.Address))
                throw new BadRequestException("Address is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.postalCode))
                throw new BadRequestException("Postal code is required.");
            if (string.IsNullOrWhiteSpace(providerRequest.taxNumber))
                throw new BadRequestException("Tax number is required.");

            if (providerRequest.LicenseFile == null)
                throw new BadRequestException("License file is required.");

            if (providerRequest.LicenseFile.Length > 5 * 1024 * 1024)
                throw new BadRequestException("License file must not exceed 5 MB.");

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(providerRequest.LicenseFile.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                throw new BadRequestException("License file must be PDF, JPG or PNG.");

            if (!Enum.IsDefined(typeof(ProviderType), providerRequest.ProviderType))
                throw new BadRequestException("Invalid provider type.");
        }
    }
}
