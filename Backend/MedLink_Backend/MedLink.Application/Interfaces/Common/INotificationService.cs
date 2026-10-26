using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationService
    {
        Task<AnnouncementResponseDto> CreateAnnouncementAsync(string adminUserId, CreateAnnouncementDto dto, CancellationToken cancellationToken = default);
        Task<PaginatedResultDto<AnnouncementResponseDto>> GetMyAnnouncementsAsync(string adminUserId, int pageNum, int pageSize, CancellationToken cancellationToken = default);
        Task DeleteMyAnnouncementAsync(string adminUserId, Guid id, CancellationToken cancellationToken = default);

        Task<PaginatedResultDto<NotificationDto>> GetMyNotificationsAsync(string userId, int pageNum, int pageSize, bool unreadOnly, CancellationToken cancellationToken = default);
        Task<UnreadCountDto> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(string userId, Guid recipientId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
        Task HideAsync(string userId, Guid recipientId, CancellationToken cancellationToken = default);
    }
}