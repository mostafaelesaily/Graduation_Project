using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Notifications;
using MedLink.Domain.Entities.Common.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MedLink.Application.Services.Common
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<AppUser> userManager;
        private readonly INotificationNotifier notifier;
        private readonly ILogger<NotificationService> logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            UserManager<AppUser> userManager,
            INotificationNotifier notifier,
            ILogger<NotificationService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.notifier = notifier;
            this.logger = logger;
        }

        public async Task<AnnouncementResponseDto> CreateAnnouncementAsync(
            string adminUserId,
            CreateAnnouncementDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new BadRequestException("Title is required.");
            if (dto.Title.Length > Notification.TitleMaxLength)
                throw new BadRequestException($"Title must not exceed {Notification.TitleMaxLength} characters.");
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new BadRequestException("Message is required.");
            if (dto.Message.Length > Notification.MessageMaxLength)
                throw new BadRequestException($"Message must not exceed {Notification.MessageMaxLength} characters.");
            if (dto.Link != null && dto.Link.Length > Notification.LinkMaxLength)
                throw new BadRequestException($"Link must not exceed {Notification.LinkMaxLength} characters.");
            if (!Enum.IsDefined(typeof(NotificationAudience), dto.TargetAudience))
                throw new BadRequestException("Invalid target audience.");

            var recipientUserIds = await GetRecipientUserIdsByAudienceAsync(dto.TargetAudience);
            var createdAt = DateTime.UtcNow;

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Source = NotificationSource.Admin,
                Type = "AdminAnnouncement",
                Title = dto.Title.Trim(),
                Message = dto.Message.Trim(),
                Link = string.IsNullOrWhiteSpace(dto.Link) ? null : dto.Link.Trim(),
                TargetAudience = dto.TargetAudience,
                CreatedByUserId = adminUserId,
                CreatedAtUtc = createdAt,
                Recipients = recipientUserIds.Select(userId => new NotificationRecipient
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAtUtc = createdAt
                }).ToList()
            };

            await unitOfWork.Repository<Notification>().AddAsync(notification);
            await unitOfWork.SaveChangesAsync();

            foreach (var recipient in notification.Recipients)
            {
                var recipientDto = new NotificationDto
                {
                    Id = recipient.Id,
                    NotificationId = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Link = notification.Link,
                    Source = notification.Source,
                    TargetAudience = notification.TargetAudience,
                    CreatedAtUtc = recipient.CreatedAtUtc,
                    IsRead = false,
                    IsHidden = false
                };

                try
                {
                    await notifier.NotifyUserAsync(recipient.UserId, recipientDto, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SignalR delivery failed for notification {NotificationId} to user {UserId}.", notification.Id, recipient.UserId);
                }
            }

            return new AnnouncementResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Link = notification.Link,
                TargetAudience = notification.TargetAudience,
                CreatedAtUtc = notification.CreatedAtUtc,
                RecipientCount = notification.Recipients.Count
            };
        }

        public async Task<PaginatedResultDto<AnnouncementResponseDto>> GetMyAnnouncementsAsync(
            string adminUserId,
            int pageNum,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNum < 1 || pageSize < 1)
                throw new ArgumentException("Page number and page size must be greater than zero.");

            var notifications = await unitOfWork.Repository<Notification>().GetAllAsync();
            var recipientCounts = (await unitOfWork.Repository<NotificationRecipient>().GetAllAsync())
                .GroupBy(r => r.NotificationId)
                .ToDictionary(g => g.Key, g => g.Count());

            var myAnnouncements = notifications
                .Where(n => n.Source == NotificationSource.Admin && n.CreatedByUserId == adminUserId)
                .OrderByDescending(n => n.CreatedAtUtc)
                .ToList();

            var totalCount = myAnnouncements.Count;
            var page = myAnnouncements
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new AnnouncementResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Link = n.Link,
                    TargetAudience = n.TargetAudience,
                    CreatedAtUtc = n.CreatedAtUtc,
                    RecipientCount = recipientCounts.TryGetValue(n.Id, out var count) ? count : 0
                })
                .ToList();

            return new PaginatedResultDto<AnnouncementResponseDto>
            {
                Data = page,
                PageNumber = pageNum,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task DeleteMyAnnouncementAsync(
            string adminUserId,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var notification = await unitOfWork.Repository<Notification>().GetByIdAsync(id);
            if (notification == null)
                throw new NotFoundException($"Notification with id {id} not found.");
            if (notification.Source != NotificationSource.Admin || notification.CreatedByUserId != adminUserId)
                throw new ForbiddenException("You are not allowed to delete this announcement.");

            unitOfWork.Repository<Notification>().Delete(notification);
            await unitOfWork.SaveChangesAsync();
        }

        public async Task<PaginatedResultDto<NotificationDto>> GetMyNotificationsAsync(
            string userId,
            int pageNum,
            int pageSize,
            bool unreadOnly,
            CancellationToken cancellationToken = default)
        {
            if (pageNum < 1 || pageSize < 1)
                throw new ArgumentException("Page number and page size must be greater than zero.");

            var recipients = await unitOfWork.Repository<NotificationRecipient>().GetAllAsync();
            var notifications = await unitOfWork.Repository<Notification>().GetAllAsync();
            var notificationLookup = notifications.ToDictionary(n => n.Id);

            var query = recipients
                .Where(r => r.UserId == userId && !r.IsHidden)
                .Where(r => !unreadOnly || !r.IsRead)
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToList();

            var totalCount = query.Count;
            var page = query
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .Select(r =>
                {
                    var n = notificationLookup[r.NotificationId];
                    return new NotificationDto
                    {
                        Id = r.Id,
                        NotificationId = n.Id,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        Link = n.Link,
                        Source = n.Source,
                        TargetAudience = n.TargetAudience,
                        CreatedAtUtc = r.CreatedAtUtc,
                        IsRead = r.IsRead,
                        ReadAtUtc = r.ReadAtUtc,
                        IsHidden = r.IsHidden
                    };
                })
                .ToList();

            return new PaginatedResultDto<NotificationDto>
            {
                Data = page,
                PageNumber = pageNum,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UnreadCountDto> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
        {
            var recipients = await unitOfWork.Repository<NotificationRecipient>().GetAllAsync();
            var count = recipients.Count(r => r.UserId == userId && !r.IsHidden && !r.IsRead);
            return new UnreadCountDto { Count = count };
        }

        public async Task MarkAsReadAsync(string userId, Guid recipientId, CancellationToken cancellationToken = default)
        {
            var recipient = await unitOfWork.Repository<NotificationRecipient>().GetByIdAsync(recipientId);
            if (recipient == null || recipient.UserId != userId)
                throw new NotFoundException($"Notification with id {recipientId} not found.");

            if (!recipient.IsRead)
            {
                recipient.IsRead = true;
                recipient.ReadAtUtc = DateTime.UtcNow;
                unitOfWork.Repository<NotificationRecipient>().Update(recipient);
                await unitOfWork.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            var recipients = await unitOfWork.Repository<NotificationRecipient>().GetAllAsync();
            var unread = recipients.Where(r => r.UserId == userId && !r.IsHidden && !r.IsRead).ToList();
            foreach (var recipient in unread)
            {
                recipient.IsRead = true;
                recipient.ReadAtUtc = DateTime.UtcNow;
                unitOfWork.Repository<NotificationRecipient>().Update(recipient);
            }
            if (unread.Count > 0)
                await unitOfWork.SaveChangesAsync();
        }

        public async Task HideAsync(string userId, Guid recipientId, CancellationToken cancellationToken = default)
        {
            var recipient = await unitOfWork.Repository<NotificationRecipient>().GetByIdAsync(recipientId);
            if (recipient == null || recipient.UserId != userId)
                throw new NotFoundException($"Notification with id {recipientId} not found.");

            recipient.IsHidden = true;
            recipient.HiddenAtUtc = DateTime.UtcNow;
            unitOfWork.Repository<NotificationRecipient>().Update(recipient);
            await unitOfWork.SaveChangesAsync();
        }

        private async Task<List<string>> GetRecipientUserIdsByAudienceAsync(NotificationAudience audience)
        {
            // Best-effort audience resolution within the Notifications feature.
            // Ordinary registration does not currently assign Roles.User, so the
            // previous Roles.User lookup was not reliably populated. To preserve
            // authentication/registration behavior, audience targeting now uses
            // active non-admin/non-provider accounts instead. This is a best-effort
            // recipient rule, not a guaranteed patient identifier.
            var userIds = new HashSet<string>();

            var adminUsers = await userManager.GetUsersInRoleAsync(Roles.Admin);
            var providerRoleUsers = await userManager.GetUsersInRoleAsync(Roles.Provider);
            var excludedUserIds = new HashSet<string>(
                adminUsers.Select(user => user.Id)
                    .Concat(providerRoleUsers.Select(user => user.Id)));

            var includeUsers = audience == NotificationAudience.User || audience == NotificationAudience.Both;
            var includeProviders = audience == NotificationAudience.Provider || audience == NotificationAudience.Both;

            if (includeUsers)
            {
                foreach (var user in userManager.Users)
                {
                    if (!user.IsActice)
                        continue;
                    if (excludedUserIds.Contains(user.Id))
                        continue;
                    userIds.Add(user.Id);
                }
            }

            if (includeProviders)
            {
                foreach (var provider in providerRoleUsers.Where(user => user.IsActice))
                    userIds.Add(provider.Id);
            }

            return userIds.ToList();
        }
    }
}