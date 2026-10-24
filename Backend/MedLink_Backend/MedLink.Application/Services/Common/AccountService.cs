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
using MedLink.Application.Constants;
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
            var confirmationLink = $"https://localhost:7048/api/Account/ConfirmMail?userId={user.Id}&token={Uri.EscapeDataString(token)}";
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

        private static readonly string[] AllowedLicenseExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private const long MaxLicenseSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public async Task<RegisterProviderResponse> RegisterProvider(RegisterProviderRequest providerRequest)
        {


            if (providerRequest == null) throw new ArgumentNullException(nameof(providerRequest));
            logger.LogInformation("Registering new provider with email: {Email}", providerRequest.Email);

            // 1. Validation
            ValidateProviderRequest(providerRequest);

            // 2. تكرار الإيميل / اليوزرنيم
            if (await userManager.FindByEmailAsync(providerRequest.Email) != null)
                throw new ConflictException($"Email {providerRequest.Email} is already registered.");
            if (await userManager.FindByNameAsync(providerRequest.userName) != null)
                throw new ConflictException($"Username {providerRequest.userName} is already taken.");

            // 3. رفع الملف
            var licensePath = await fileService.UploadFileAsync(providerRequest.LicenseFile, "licenses");

            AppUser user;
            await unitOfWork.BeginTransactionAsync();
            try
            {
                // 4. حفظ بيانات الملف
                var storedFile = new StoredFile
                {
                    FileName = providerRequest.LicenseFile.FileName,
                    FilePath = licensePath,
                    FileType = providerRequest.LicenseFile.ContentType
                };
                await unitOfWork.Repository<StoredFile>().AddAsync(storedFile);

                // 5. Mapping وربط الكيانات
                user = mapper.Map<AppUser>(providerRequest);
                var profile = mapper.Map<ProviderProfile>(providerRequest);
                var location = mapper.Map<ProviderLocations>(providerRequest);

                profile.LicenseFileId = storedFile.Id;
                profile.ProviderLocations.Add(location);
                user.providerProfile = profile;

                // 6. إنشاء اليوزر (بيحفظ الـ Profile والـ Location معاه)
                var createResult = await userManager.CreateAsync(user, providerRequest.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new BadRequestException("Provider registration failed. Errors: " + errors);
                }

                // 7. إضافة الـ Role
                var roleResult = await userManager.AddToRoleAsync(user, Roles.Provider);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    throw new BadRequestException("Failed to assign Provider role. Errors: " + errors);
                }

                await unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                await fileService.DeleteAsync(licensePath);   // منسيبش ملف يتيم
                throw;
            }

            // 8. إيميل التأكيد (بعد الـ Commit)
            await SendConfirmationEmailAsync(user);

            logger.LogInformation("Provider registered successfully with email: {Email}", user.Email);
            return new RegisterProviderResponse
            {
                message = "Provider registered successfully. Please confirm your email, then wait for admin approval.",
                Email = user.Email!,
                ApprovalStatus = VerificationStatus.Pending
            };
        }

        private static void ValidateProviderRequest(RegisterProviderRequest r)
        {
            if (string.IsNullOrWhiteSpace(r.Email)) throw new BadRequestException("Email is required.");
            if (string.IsNullOrWhiteSpace(r.userName)) throw new BadRequestException("Username is required.");
            if (string.IsNullOrWhiteSpace(r.Password)) throw new BadRequestException("Password is required.");
            if (string.IsNullOrWhiteSpace(r.phoneNumber)) throw new BadRequestException("Phone number is required.");
            if (string.IsNullOrWhiteSpace(r.ProviderName)) throw new BadRequestException("Provider name is required.");
            if (string.IsNullOrWhiteSpace(r.Address)) throw new BadRequestException("Address is required.");
            if (string.IsNullOrWhiteSpace(r.postalCode)) throw new BadRequestException("Postal code is required.");
            if (string.IsNullOrWhiteSpace(r.taxNumber)) throw new BadRequestException("Tax number is required.");
            if (!Enum.IsDefined(typeof(ProviderType), r.ProviderType))
                throw new BadRequestException("Invalid provider type.");

            if (r.LicenseFile == null || r.LicenseFile.Length == 0)
                throw new BadRequestException("License file is required.");
            if (r.LicenseFile.Length > MaxLicenseSizeInBytes)
                throw new BadRequestException("License file must not exceed 5 MB.");

            var extension = Path.GetExtension(r.LicenseFile.FileName).ToLowerInvariant();
            if (!AllowedLicenseExtensions.Contains(extension))
                throw new BadRequestException("License file must be PDF, JPG or PNG.");
        }

        private async Task SendConfirmationEmailAsync(AppUser user)
        {
            try
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var baseUrl = configuration["App:BaseUrl"] ?? "https://localhost:44302";
                var link = $"{baseUrl}/api/Account/ConfirmMail?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                await mailService.sendMail(
                    user.Email!,
                    "Confirm your email",
                    $"Please confirm your account by clicking this link: <a href='{link}'>Confirm Email</a>");

                logger.LogInformation("Confirmation email sent successfully to: {Email}", user.Email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email to: {Email}", user.Email);
            }

        }

      
    }
}
