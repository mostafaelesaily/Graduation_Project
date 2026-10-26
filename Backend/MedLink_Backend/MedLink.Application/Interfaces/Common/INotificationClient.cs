using MedLink.Application.DTOs.Common.NotificationDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationDto notification);
    }
}