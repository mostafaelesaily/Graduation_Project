using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationService
    {
        Task<PaginatedResultDto<GetNotificationDto>> GetMyNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task<PaginatedResultDto<GetNotificationDto>> GetMyUnreadNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task SendToUserAsync(string userId, CreateNotificationDto notification);
        Task SendToGroupAsync(string groupName, CreateNotificationDto notification);
        Task MarkAsReadAsync(Guid notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
