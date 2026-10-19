using MedLink.Application.DTOs.Common.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateAsync(
            string userId,
            string title,
            string message,
            string type,
            string? link = null);

        Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int page = 1,
            int pageSize = 20);

        Task<int> GetUnreadCountAsync(string userId);

        Task<bool> MarkAsReadAsync(
            Guid notificationId, // تم التعديل إلى Guid
            string userId);

        // returns how many notifications were updated
        Task<int> MarkAllAsReadAsync(string userId);
    }
}