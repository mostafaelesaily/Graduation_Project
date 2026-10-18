using MedLink.Application.DTOs.Common.Notifications;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationPublisher
    {
        Task PublishAsync(string userId, NotificationDto notification);
    }
}