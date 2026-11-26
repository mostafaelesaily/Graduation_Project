using MedLink.Application.Constants;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MedLink.Presentation.Hubs;

[Authorize]
public class NotificationHub : Hub<INotificationClient>
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User!;

        var userId = Context.UserIdentifier;

        // Personal group
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"User_{userId}");
        }

        // Role groups
        if (user.IsInRole(Roles.User))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                Roles.User);
        }

        if (user.IsInRole(Roles.Provider))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                Roles.Provider);
        }

        if (user.IsInRole(Roles.Admin))
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                Roles.Admin);
        }

        await base.OnConnectedAsync();
    }
}