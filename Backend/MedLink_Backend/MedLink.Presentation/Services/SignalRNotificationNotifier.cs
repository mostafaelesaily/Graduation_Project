using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.Interfaces.Common;
using MedLink.Presentation.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace MedLink.Presentation.Services
{
    public class SignalRNotificationNotifier : INotificationNotifier
    {
        private readonly IHubContext<NotificationHub, INotificationClient> hubContext;

        public SignalRNotificationNotifier(IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        public async Task NotifyUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            await hubContext.Clients
                .Group($"user:{userId}")
                .ReceiveNotification(notification);
        }
    }
}