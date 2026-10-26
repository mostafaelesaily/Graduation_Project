using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
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
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        private string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedException("User identity is not available.");
            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<NotificationDto>>> GetMyNotifications(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool unreadOnly = false,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            var result = await notificationService.GetMyNotificationsAsync(userId, pageNum, pageSize, unreadOnly, cancellationToken);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountDto>> GetUnreadCount(
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            var result = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            await notificationService.MarkAsReadAsync(userId, id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Hide(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            var userId = GetUserId();
            await notificationService.HideAsync(userId, id, cancellationToken);
            return NoContent();
        }
    }
}