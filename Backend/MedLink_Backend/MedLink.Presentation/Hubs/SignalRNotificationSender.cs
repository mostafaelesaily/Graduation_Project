using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.AspNetCore.SignalR;

namespace MedLink.Presentation.Hubs;

public class SignalRNotificationSender : INotificationSender
{
    public SignalRNotificationSender(IHubContext<NotificationHub, INotificationClient> hubContext)
    {
        this.hubContext = hubContext;
    }

    private readonly IHubContext<NotificationHub, INotificationClient> hubContext;

    public Task SendToUserAsync(string userId, GetNotificationDto notification)
    {
        return hubContext.Clients.Group($"User_{userId}").ReceiveNotification(notification);
    }

    public Task SendToGroupAsync(string groupName, GetNotificationDto notification)
    {
        return hubContext.Clients.Group(groupName).ReceiveNotification(notification);
    }
}
