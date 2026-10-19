using MedLink.Application.DTOs.Common.Notifications;
using MedLink.Application.Interfaces.Common;
using MedLink.Domain.Entities.Common.Notifications;
using MedLink.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MedLink.infrastructure.Services.Common
{
    public class NotificationService : INotificationService
    {
        private const int MaxPageSize = 50;

        private readonly AppDbContext _context;
        private readonly INotificationPublisher _publisher;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            AppDbContext context,
            INotificationPublisher publisher,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<NotificationDto> CreateAsync(
            string userId,
            string title,
            string message,
            string type,
            string? link = null)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(), // ضمان توليد الـ Guid الجديد
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                Link = link,
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            // 1) Save FIRST: the notification must never be lost, even if the user is offline.
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var dto = MapToDto(notification);

            // 2) Then push in real time. A push failure must NOT fail the operation.
            try
            {
                await _publisher.PublishAsync(userId, dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Notification {NotificationId} was saved but the realtime push to user {UserId} failed.",
                    notification.Id, userId);
            }

            return dto;
        }

       

        public async Task<IReadOnlyList<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int page = 1,
            int pageSize = 20)
        {
            // Never trust the numbers that come from the client.
            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .ThenByDescending(n => n.Id)   // stable order, so pages never overlap
                .Skip((page - 1) * pageSize)   // تم التصحيح هنا (page - 1)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAtUtc = n.CreatedAtUtc,
                    ReadAtUtc = n.ReadAtUtc,
                    Link = n.Link
                })
                .ToListAsync();
        }
        public Task<int> GetUnreadCountAsync(string userId)
        {
            return _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<bool> MarkAsReadAsync(
            Guid notificationId, // تم التعديل إلى Guid
            string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.Id == notificationId &&
                    n.UserId == userId);

            if (notification is null)
            {
                return false;
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<int> MarkAllAsReadAsync(string userId)
        {
            // One UPDATE statement in the database, without loading the rows into memory.
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAtUtc, DateTime.UtcNow));
        }

        private static NotificationDto MapToDto(
            Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAtUtc = notification.CreatedAtUtc,
                ReadAtUtc = notification.ReadAtUtc,
                Link = notification.Link
            };
        }
    }
}