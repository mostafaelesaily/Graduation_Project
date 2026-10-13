using System.Security.Claims;
using MedLink.Application.DTOs.Common.identity;
using MedLink.Application.Interfaces.Common;
using MedLink.Presentation.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MedLink.Test.Controllers
{
    public class AccountControllerTest
    {
        private readonly Mock<IAccountService> accountServiceMock = new();
        private readonly AccountController sut;

        public AccountControllerTest()
        {
            sut = new AccountController(accountServiceMock.Object);
        }

        private static void SetUser(AccountController controller, string userId)
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId) }, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        // ---------- Register ----------

        [Fact]
        public async Task Register_ValidDto_ReturnsOkWithServiceResult()
        {
            var dto = new RegisterDto { Email = "test@medlink.com", Password = "Pass123!", userName = "test", phoneNumber = "0100000000" };
            var expected = new RegisterResponseDto { message = "User Registered Successfully" };
            accountServiceMock.Setup(s => s.Register(dto)).ReturnsAsync(expected);

            var result = await sut.Register(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
            accountServiceMock.Verify(s => s.Register(dto), Times.Once);
        }

        [Fact]
        public async Task Register_ServiceThrows_ExceptionPropagates()
        {
            var dto = new RegisterDto { Email = "test@medlink.com", Password = "weak", userName = "test", phoneNumber = "0100000000" };
            accountServiceMock.Setup(s => s.Register(dto)).ThrowsAsync(new Exception("PasswordTooShort: too short."));

            await Assert.ThrowsAsync<Exception>(() => sut.Register(dto));
        }

        // ---------- Login ----------

        [Fact]
        public async Task Login_ValidDto_ReturnsOkWithServiceResult()
        {
            var dto = new LoginDto { Email = "test@medlink.com", Password = "Pass123!" };
            var expected = new AuthenticationResponseDto { AccessToken = "access", RefreshToken = "refresh", Message = "Login successful" };
            accountServiceMock.Setup(s => s.Login(dto)).ReturnsAsync(expected);

            var result = await sut.Login(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task Login_ServiceThrows_ExceptionPropagates()
        {
            var dto = new LoginDto { Email = "test@medlink.com", Password = "wrong" };
            accountServiceMock.Setup(s => s.Login(dto)).ThrowsAsync(new Exception("Invalid Password"));

            await Assert.ThrowsAsync<Exception>(() => sut.Login(dto));
        }

        // ---------- HandleRefreshToken ----------

        [Fact]
        public async Task HandleRefreshToken_ValidDto_ReturnsOkWithServiceResult()
        {
            var dto = new GenrateRefreshToken { Token = "old-token" };
            var expected = new AuthenticationResponseDto { AccessToken = "access", RefreshToken = "new-token", Message = "Token refreshed successfully" };
            accountServiceMock.Setup(s => s.HandleRefreshTokenAsync(dto)).ReturnsAsync(expected);

            var result = await sut.HandleRefreshToken(dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected, okResult.Value);
        }

        // ---------- ConfirmMail ----------

        [Fact]
        public async Task ConfirmMail_ValidParams_ReturnsOkWithServiceResult()
        {
            accountServiceMock.Setup(s => s.ConfirmMail("user-1", "token-1")).ReturnsAsync("Email confirmed successfully.");

            var result = await sut.ConfirmMail("user-1", "token-1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Email confirmed successfully.", okResult.Value);
            accountServiceMock.Verify(s => s.ConfirmMail("user-1", "token-1"), Times.Once);
        }

        // ---------- Logout ----------

        [Fact]
        public async Task Logout_AuthenticatedUser_CallsServiceWithUserIdFromClaims()
        {
            SetUser(sut, "user-42");
            accountServiceMock.Setup(s => s.Logout("user-42")).Returns(Task.CompletedTask);

            var result = await sut.Logout();

            Assert.IsType<OkObjectResult>(result);
            accountServiceMock.Verify(s => s.Logout("user-42"), Times.Once);
        }

        [Fact]
        public async Task Logout_ServiceThrows_ExceptionPropagates()
        {
            SetUser(sut, "user-42");
            accountServiceMock.Setup(s => s.Logout("user-42")).ThrowsAsync(new Exception("User Not Found"));

            await Assert.ThrowsAsync<Exception>(() => sut.Logout());
        }
    }
}
