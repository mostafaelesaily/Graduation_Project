using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using MedLink.Application.DTOs.Common.identity;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Test.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace MedLink.Test.Services
{
    public class AccountServiceTest
    {
        private readonly Mock<IMapper> mapperMock = new();
        private readonly Mock<IConfiguration> configurationMock = new();
        private readonly Mock<ILogger<AccountService>> loggerMock = new();
        private readonly Mock<UserManager<AppUser>> userManagerMock;
        private readonly Mock<IMailService> mailServiceMock = new();
        private readonly AccountService sut;

        public AccountServiceTest()
        {
            var storeMock = new Mock<IUserStore<AppUser>>();
            userManagerMock = new Mock<UserManager<AppUser>>(
                storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            configurationMock.Setup(c => c["JWT:Key"]).Returns("Super_Secret_Test_Key_1234567890_ABCDEFGH");
            configurationMock.Setup(c => c["JWT:Issuer"]).Returns("test-issuer");
            configurationMock.Setup(c => c["JWT:Audience"]).Returns("test-audience");

            sut = new AccountService(
                mapperMock.Object,
                configurationMock.Object,
                loggerMock.Object,
                userManagerMock.Object,
                mailServiceMock.Object);
        }

        private static AppUser CreateUser(string id = "user-1", string email = "test@medlink.com", bool emailConfirmed = true)
        {
            return new AppUser
            {
                Id = id,
                Email = email,
                UserName = email,
                EmailConfirmed = emailConfirmed,
                RefreshTokens = new List<RefreshTokens>()
            };
        }

        // ---------- GenrateAccessToken ----------

        [Fact]
        public async Task GenrateAccessToken_ValidUser_ReturnsTokenWithExpectedClaims()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Patient" });

            var token = await sut.GenrateAccessToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.Equal(user.Email, jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Email).Value);
            Assert.Equal("Patient", jwt.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.Role).Value);
            Assert.Equal("test-issuer", jwt.Issuer);
        }

        // ---------- GenrateRefreshToken ----------

        [Fact]
        public async Task GenrateRefreshToken_ReturnsTokenValidForSevenDays()
        {
            var result = await sut.GenrateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.True((result.ExpiresOn - DateTime.UtcNow).TotalDays is > 6.9 and < 7.1);
        }

        // ---------- HandleRefreshTokenAsync ----------

        [Fact]
        public async Task HandleRefreshTokenAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleRefreshTokenAsync(null!));
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_TokenNotFound_ThrowsUnauthorizedException()
        {
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser>().AsTestAsyncQueryable());

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => sut.HandleRefreshTokenAsync(new GenrateRefreshToken { Token = "missing-token" }));
            Assert.Equal("Invalid token", ex.Message);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_ExpiredToken_ThrowsUnauthorizedException()
        {
            var user = CreateUser();
            user.RefreshTokens.Add(new RefreshTokens
            {
                Token = "expired-token",
                CreatedOn = DateTime.UtcNow.AddDays(-10),
                ExpiresOn = DateTime.UtcNow.AddDays(-3)
            });
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsTestAsyncQueryable());

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => sut.HandleRefreshTokenAsync(new GenrateRefreshToken { Token = "expired-token" }));
            Assert.Equal("Invalid token", ex.Message);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_RevokedToken_ThrowsUnauthorizedException()
        {
            var user = CreateUser();
            user.RefreshTokens.Add(new RefreshTokens
            {
                Token = "revoked-token",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                ExpiresOn = DateTime.UtcNow.AddDays(6),
                revokedOn = DateTime.UtcNow
            });
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsTestAsyncQueryable());

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => sut.HandleRefreshTokenAsync(new GenrateRefreshToken { Token = "revoked-token" }));
            Assert.Equal("Invalid token", ex.Message);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_ValidToken_RevokesOldAndReturnsNewTokens()
        {
            var user = CreateUser();
            var oldToken = new RefreshTokens
            {
                Token = "valid-token",
                CreatedOn = DateTime.UtcNow.AddDays(-1),
                ExpiresOn = DateTime.UtcNow.AddDays(6)
            };
            user.RefreshTokens.Add(oldToken);
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsTestAsyncQueryable());
            userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await sut.HandleRefreshTokenAsync(new GenrateRefreshToken { Token = "valid-token" });

            Assert.NotNull(oldToken.revokedOn);
            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.NotEqual("valid-token", result.RefreshToken);
            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        // ---------- Login ----------

        [Fact]
        public async Task Login_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => sut.Login(null!));
        }

        [Fact]
        public async Task Login_UserNotFound_ThrowsNotFoundException()
        {
            userManagerMock.Setup(x => x.FindByEmailAsync("missing@medlink.com")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => sut.Login(new LoginDto { Email = "missing@medlink.com", Password = "Pass123!" }));
            Assert.Equal("User with email missing@medlink.com not found.", ex.Message);
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsUnauthorizedException()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.CheckPasswordAsync(user, "WrongPass")).ReturnsAsync(false);

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => sut.Login(new LoginDto { Email = user.Email!, Password = "WrongPass" }));
            Assert.Equal("Invalid Email Or Password", ex.Message);
        }

        [Fact]
        public async Task Login_EmailNotConfirmed_ThrowsUnauthorizedException()
        {
            var user = CreateUser(emailConfirmed: false);
            userManagerMock.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);

            var ex = await Assert.ThrowsAsync<UnauthorizedException>(
                () => sut.Login(new LoginDto { Email = user.Email!, Password = "Pass123!" }));
            Assert.Equal("Email not confirmed. Please confirm your email before logging in.", ex.Message);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsAuthenticationResponse()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.FindByEmailAsync(user.Email!)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);
            userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string>());
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await sut.Login(new LoginDto { Email = user.Email!, Password = "Pass123!" });

            Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
            Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
            Assert.Equal("Login successful", result.Message);
            Assert.Single(user.RefreshTokens);
        }

        // ---------- Register ----------

        [Fact]
        public async Task Register_NullDto_ThrowsNullReferenceException()
        {
            // NOTE: AccountService.Register logs `appUser.Email` before checking `appUser == null`,
            // so a null DTO throws NullReferenceException instead of the intended ArgumentNullException.
            // This test documents the current (buggy) behavior; update it once the null-check order is fixed.
            await Assert.ThrowsAsync<NullReferenceException>(() => sut.Register(null!));
        }

        [Fact]
        public async Task Register_CreateFails_ThrowsBadRequestExceptionWithErrors()
        {
            var registerDto = new RegisterDto { Email = "new@medlink.com", Password = "weak", userName = "new", phoneNumber = "0100000000" };
            var mappedUser = CreateUser(email: registerDto.Email);
            mapperMock.Setup(m => m.Map<AppUser>(registerDto)).Returns(mappedUser);
            userManagerMock.Setup(x => x.CreateAsync(mappedUser, registerDto.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordTooShort", Description = "Password too short." }));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => sut.Register(registerDto));
            Assert.Contains("Password too short.", ex.Message);
        }

        [Fact]
        public async Task Register_Success_SendsConfirmationEmailAndReturnsSuccessMessage()
        {
            var registerDto = new RegisterDto { Email = "new@medlink.com", Password = "Pass123!", userName = "new", phoneNumber = "0100000000" };
            var mappedUser = CreateUser(email: registerDto.Email);
            mapperMock.Setup(m => m.Map<AppUser>(registerDto)).Returns(mappedUser);
            userManagerMock.Setup(x => x.CreateAsync(mappedUser, registerDto.Password)).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(mappedUser)).ReturnsAsync("confirm-token");
            mailServiceMock.Setup(x => x.sendMail(mappedUser.Email!, It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var result = await sut.Register(registerDto);

            Assert.Equal("User Registered Successfully", result.message);
            mailServiceMock.Verify(x => x.sendMail(mappedUser.Email!, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_MailServiceThrows_StillReturnsSuccessResponse()
        {
            var registerDto = new RegisterDto { Email = "new@medlink.com", Password = "Pass123!", userName = "new", phoneNumber = "0100000000" };
            var mappedUser = CreateUser(email: registerDto.Email);
            mapperMock.Setup(m => m.Map<AppUser>(registerDto)).Returns(mappedUser);
            userManagerMock.Setup(x => x.CreateAsync(mappedUser, registerDto.Password)).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(mappedUser)).ReturnsAsync("confirm-token");
            mailServiceMock.Setup(x => x.sendMail(mappedUser.Email!, It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP connection failed"));

            var result = await sut.Register(registerDto);

            Assert.Equal("User Registered Successfully", result.message);
        }

        // ---------- ConfirmMail ----------

        [Fact]
        public async Task ConfirmMail_UserNotFound_ThrowsNotFoundException()
        {
            userManagerMock.Setup(x => x.FindByIdAsync("missing-id")).ReturnsAsync((AppUser?)null);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => sut.ConfirmMail("missing-id", "token"));
            Assert.Equal("User Not Found", ex.Message);
        }

        [Fact]
        public async Task ConfirmMail_InvalidToken_ThrowsBadRequestExceptionWithErrors()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.ConfirmEmailAsync(user, "bad-token"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "InvalidToken", Description = "Invalid token." }));

            var ex = await Assert.ThrowsAsync<BadRequestException>(() => sut.ConfirmMail(user.Id, "bad-token"));
            Assert.Contains("Invalid token.", ex.Message);
        }

        [Fact]
        public async Task ConfirmMail_ValidToken_ReturnsSuccessMessage()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            userManagerMock.Setup(x => x.ConfirmEmailAsync(user, "good-token")).ReturnsAsync(IdentityResult.Success);

            var result = await sut.ConfirmMail(user.Id, "good-token");

            Assert.Equal("Email confirmed successfully.", result);
        }

        // ---------- Logout ----------

        [Fact]
        public async Task Logout_UserNotFound_ThrowsNotFoundException()
        {
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser>().AsTestAsyncQueryable());

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => sut.Logout("missing-id"));
            Assert.Equal("User Not Found", ex.Message);
        }

        [Fact]
        public async Task Logout_Success_RevokesAllActiveRefreshTokens()
        {
            var user = CreateUser();
            user.RefreshTokens.Add(new RefreshTokens { Token = "t1", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddDays(7) });
            user.RefreshTokens.Add(new RefreshTokens { Token = "t2", CreatedOn = DateTime.UtcNow, ExpiresOn = DateTime.UtcNow.AddDays(7) });
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsTestAsyncQueryable());
            userManagerMock.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            await sut.Logout(user.Id);

            Assert.All(user.RefreshTokens, t => Assert.NotNull(t.revokedOn));
            userManagerMock.Verify(x => x.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task Logout_UpdateFails_ThrowsException()
        {
            var user = CreateUser();
            userManagerMock.Setup(x => x.Users).Returns(new List<AppUser> { user }.AsTestAsyncQueryable());
            userManagerMock.Setup(x => x.UpdateAsync(user))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Concurrency failure." }));

            var ex = await Assert.ThrowsAsync<Exception>(() => sut.Logout(user.Id));
            Assert.Equal("Failed to logout user", ex.Message);
        }
    }
}
