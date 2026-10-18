using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedLink.Presentation.Hubs
{
    // [Authorize]: nobody can open a connection without a valid JWT.
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            // Context.UserIdentifier = the user id taken from the JWT (ClaimTypes.NameIdentifier)
            logger.LogInformation(
                "SignalR connected. UserId: {UserId}, ConnectionId: {ConnectionId}",
                Context.UserIdentifier,
                Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            logger.LogInformation(
                "SignalR disconnected. UserId: {UserId}, ConnectionId: {ConnectionId}",
                Context.UserIdentifier,
                Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}