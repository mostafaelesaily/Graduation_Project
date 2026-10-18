using MedLink.Application.DTOs.Common.Notifications;
using MedLink.Application.Interfaces.Common;
using MedLink.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MedLink.Presentation.Realtime
{
    public class SignalRNotificationPublisher : INotificationPublisher
    {
        private readonly IHubContext<NotificationHub> hubContext;

        public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task PublishAsync(string userId, NotificationDto notification)
        {
            // Clients.User(userId) reaches ALL connections of this user
            // (several tabs / devices). If the user is offline, nothing happens.
            await hubContext.Clients
                .User(userId)
                .SendAsync(NotificationEvents.ReceiveNotification, notification);
        }
    }
}