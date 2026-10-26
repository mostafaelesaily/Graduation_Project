using MedLink.Application.Constants;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MedLink.Presentation.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");

                if (Context.User?.IsInRole(Roles.Admin) == true)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");

                if (Context.User?.IsInRole(Roles.Admin) == true)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "admin");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}