using MedLink.Application.DTOs.Common.NotificationDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    // Application-layer abstraction for real-time notification delivery.
    // Implemented in Presentation using SignalR (IHubContext<NotificationHub, INotificationClient>)
    // so that Application never references SignalR or the Hub directly.
    public interface INotificationSender
    {
        Task SendToUserAsync(string userId, GetNotificationDto notification);
        Task SendToGroupAsync(string groupName, GetNotificationDto notification);
    }
}
