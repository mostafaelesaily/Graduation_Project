using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        public NotificationController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        private readonly INotificationService notificationService;

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpGet("[action]")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await notificationService.GetMyNotificationsAsync(CurrentUserId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetMyUnreadNotifications(
            [FromQuery] int pageNum = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await notificationService.GetMyUnreadNotificationsAsync(CurrentUserId, pageNum, pageSize);
            return Ok(result);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await notificationService.GetUnreadCountAsync(CurrentUserId);
            return Ok(result);
        }

        [HttpPatch("{notificationId:guid}/[action]")]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid notificationId)
        {
            await notificationService.MarkAsReadAsync(notificationId, CurrentUserId);
            return NoContent();
        }

        [HttpPatch("[action]")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await notificationService.MarkAllAsReadAsync(CurrentUserId);
            return NoContent();
        }
    }
}
