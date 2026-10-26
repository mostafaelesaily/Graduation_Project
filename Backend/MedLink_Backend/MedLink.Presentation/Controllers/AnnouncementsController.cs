using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.Constants;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MedLink.Presentation.Controllers
{
    [Route("api/admin/announcements")]
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    public class AnnouncementsController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public AnnouncementsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        private string GetAdminId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedException("User identity is not available.");
            return userId;
        }

        [HttpPost]
        public async Task<ActionResult<AnnouncementResponseDto>> Create(
            [FromBody] CreateAnnouncementDto dto,
            CancellationToken cancellationToken = default)
        {
            var adminId = GetAdminId();
            var result = await notificationService.CreateAnnouncementAsync(adminId, dto, cancellationToken);
            return Created($"/api/admin/announcements/{result.Id}", result);
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<AnnouncementResponseDto>>> GetMine(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var adminId = GetAdminId();
            var result = await notificationService.GetMyAnnouncementsAsync(adminId, pageNum, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var adminId = GetAdminId();
            await notificationService.DeleteMyAnnouncementAsync(adminId, id, cancellationToken);
            return NoContent();
        }
    }
}