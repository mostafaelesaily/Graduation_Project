
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

        [HttpGet("providers/{providerId:guid}/documents")]
        public async Task<IActionResult> GetProviderDocuments([FromRoute] Guid providerId)
        {
            var result = await adminService.GetProviderDocumentsAsync(providerId);
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
            [FromRoute] Guid providerId,
            [FromQuery] string? reason = null)
        {
            await adminService.RejectProviderAsync(providerId, reason);

            return NoContent();
        }
    }
}
