
using MedLink.Application.Interfaces.Common;
using MedLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedLink.Presentation.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService adminService;

        public AdminController(IAdminService adminService)
        {
            this.adminService = adminService;
        }

        [HttpGet("provider-requests")]
        public async Task<IActionResult> GetAllProviderRequest(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] VerificationStatus? status = null)
        {
            var result = await adminService.GetAllProviderRequest(
                pageNum, pageSize, search, status);
            return Ok(result);
        }

        [HttpPatch("providers/{providerId:guid}/accept")]
        public async Task<IActionResult> AcceptProvider(
            [FromRoute] Guid providerId)
        {
            await adminService.AcceptProviderAsync(providerId);

            return NoContent();
        }

        [HttpPatch("providers/{providerId:guid}/reject")]
        public async Task<IActionResult> RejectProvider(
            [FromRoute] Guid providerId)
        {
            await adminService.RejectProviderAsync(providerId);

            return NoContent();
        }
    }
}
