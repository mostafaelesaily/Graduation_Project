using MedLink.Application.DTOs.Common.NotificationDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    // Strongly-typed SignalR client contract, used by NotificationHub (Presentation)
    // and IHubContext<NotificationHub, INotificationClient>.
    public interface INotificationClient
    {
        Task ReceiveNotification(GetNotificationDto notification);
    }
}
