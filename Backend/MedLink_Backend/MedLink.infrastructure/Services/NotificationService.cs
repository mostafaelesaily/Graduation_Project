using MedLink.Application.DTOs.Common.Notifications;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Domain.Entities.Common.Notifications;
using MedLink.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task<NotificationDto> CreateAsync(CreateNotificationDto request)
        {
            Validate(request);

            var notification = new Notification
            {
                // Version 7 = time-ordered Guid: friendlier to the database index than Guid.NewGuid()
                Id = Guid.CreateVersion7(),
                UserId = request.UserId,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                Type = request.Type.Trim(),
                Link = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim(),
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
                await _publisher.PublishAsync(notification.UserId, dto);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Notification {NotificationId} was saved but the realtime push to user {UserId} failed.",
                    notification.Id, notification.UserId);
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
                .Skip((page - 1) * pageSize)
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
            Guid notificationId,
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

        // The attributes on CreateNotificationDto are only enforced when the DTO comes from an HTTP body.
        // When another service calls CreateAsync directly nobody checks them, so we validate here too.
        private static void Validate(CreateNotificationDto request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new BadRequestException("UserId is required.");

            if (string.IsNullOrWhiteSpace(request.Title))
                throw new BadRequestException("Title is required.");
            if (request.Title.Trim().Length > Notification.TitleMaxLength)
                throw new BadRequestException($"Title must not exceed {Notification.TitleMaxLength} characters.");

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new BadRequestException("Message is required.");
            if (request.Message.Trim().Length > Notification.MessageMaxLength)
                throw new BadRequestException($"Message must not exceed {Notification.MessageMaxLength} characters.");

            if (string.IsNullOrWhiteSpace(request.Type))
                throw new BadRequestException("Type is required.");
            if (request.Type.Trim().Length > Notification.TypeMaxLength)
                throw new BadRequestException($"Type must not exceed {Notification.TypeMaxLength} characters.");

            if (request.Link is not null && request.Link.Trim().Length > Notification.LinkMaxLength)
                throw new BadRequestException($"Link must not exceed {Notification.LinkMaxLength} characters.");
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