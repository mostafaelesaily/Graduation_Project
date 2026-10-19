using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedLink.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            this.notificationService = notificationService;
        }

        // The user id ALWAYS comes from the JWT, never from the request body or the URL.
        private string CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedException("Invalid token");

        // GET api/notifications?page=1&pageSize=20
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await notificationService.GetMyNotificationsAsync(CurrentUserId, page, pageSize);
            return Ok(result);
        }

        // GET api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await notificationService.GetUnreadCountAsync(CurrentUserId);
            return Ok(new { count });
        }

        // POST api/notifications/5/read
        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var updated = await notificationService.MarkAsReadAsync(id, CurrentUserId);
            if (!updated) throw new NotFoundException("Notification not found");

            return NoContent();
        }

        // POST api/notifications/read-all
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var updated = await notificationService.MarkAllAsReadAsync(CurrentUserId);
            return Ok(new { updated });
        }

        // TESTING ONLY - creates a notification for the logged-in user.
        // Works in Development only. We will delete it when the real features use the service.
        // POST api/notifications/test
        [HttpPost("test")]
        public async Task<IActionResult> CreateTest([FromServices] IWebHostEnvironment environment)
        {
            if (!environment.IsDevelopment()) return NotFound();

            var created = await notificationService.CreateAsync(
                CurrentUserId,
                "Test notification",
                "This is a test notification",
                "Test");

            return Ok(created);
        }
    }
}