using MedLink.Application.DTOs.Common.identity;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
        }
        private readonly IAccountService accountService;

        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterDto RegisterDto)
        {
            var result = await accountService.Register(RegisterDto);
             return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await accountService.Login(loginDto);
            return Ok(result);
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> HandleRefreshToken(GenrateRefreshToken refreshToken)
        {
            var Result = await accountService.HandleRefreshTokenAsync(refreshToken);
            return Ok(Result);
        }
        [HttpGet("[action]")]
        public async Task<IActionResult> ConfirmMail(string userId , string token)
        {
            var result = await accountService.ConfirmMail(userId, token);
            return Ok(result);
        }
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await accountService.Logout(userId);
            return Ok(new { Message = "Logged out successfully" });
        }
        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterProvider([FromForm] RegisterProviderRequest request)
        {
            var result = await accountService.RegisterProvider(request);
            return Ok(result);
        }

    }
}
