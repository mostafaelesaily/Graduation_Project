using MedLink.Application.DTOs.Common.Notifications;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAsync(CreateNotificationDto request);

        Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int page = 1,
            int pageSize = 20);

        Task<int> GetUnreadCountAsync(string userId);

        Task<bool> MarkAsReadAsync(
            Guid notificationId,
            string userId);

        // returns how many notifications were updated
        Task<int> MarkAllAsReadAsync(string userId);
    }
}