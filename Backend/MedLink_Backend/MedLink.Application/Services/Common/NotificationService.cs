using AutoMapper;
using MedLink.Application.Constants;
using MedLink.Application.DTOs.Common.NotificationDTOs;
using MedLink.Application.DTOs.Common.PaginatedDTOs;
using MedLink.Application.Exceptions;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.Notifications;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MedLink.Application.Services.Common
{
    public class NotificationService : INotificationService
    {
        public NotificationService
            (
            IMapper mapper ,
            ILogger<NotificationService> logger,
            IUnitOfWork unitOfWork,
            INotificationSender notificationSender,
            UserManager<AppUser> userManager,
            ICacheService cacheService
            )
        {
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.mapper = mapper;
            this.notificationSender = notificationSender;
            this.userManager = userManager;
            this.cacheService = cacheService;
        }
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<NotificationService> logger;
        private readonly IMapper mapper;
        private readonly INotificationSender notificationSender;
        private readonly UserManager<AppUser> userManager;
        private readonly ICacheService cacheService;

        private static string UserCachePrefix(string userId) => $"Notifications_{userId}";

        public Task<PaginatedResultDto<GetNotificationDto>> GetMyNotificationsAsync(string userId, int pageNumber, int pageSize)
        {
            return GetPagedAsync(userId, unreadOnly: false, pageNumber, pageSize);
        }

        public Task<PaginatedResultDto<GetNotificationDto>> GetMyUnreadNotificationsAsync(string userId, int pageNumber, int pageSize)
        {
            return GetPagedAsync(userId, unreadOnly: true, pageNumber, pageSize);
        }

        private async Task<PaginatedResultDto<GetNotificationDto>> GetPagedAsync(string userId, bool unreadOnly, int pageNumber, int pageSize)
        {
            var cacheKey = $"{UserCachePrefix(userId)}_unread:{unreadOnly}_page:{pageNumber}_size:{pageSize}";

            var result = await cacheService.GetOrSetAsync(cacheKey, async () =>
            {
                var repo = unitOfWork.Repository<Notifications>();
                var countSpec = new NotificationsByUserSpecification(userId, unreadOnly);
                var totalCount = await repo.CountAsync(countSpec);

                var pagedSpec = new NotificationsByUserSpecification(userId, unreadOnly, pageNumber, pageSize);
                var notifications = await repo.GetAllAsync(pagedSpec);

                return new PaginatedResultDto<GetNotificationDto>
                {
                    Data = mapper.Map<IReadOnlyList<GetNotificationDto>>(notifications),
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            });

            return result;
        }

        public async Task SendToUserAsync(string userId, CreateNotificationDto notification)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new BadRequestException("Recipient user id is required.");

            var entity = new Notifications
            {
                UserId = userId,
                Title = notification.Title,
                Message = notification.Message
            };

            var repo = unitOfWork.Repository<Notifications>();
            await repo.AddAsync(entity);
            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync(UserCachePrefix(userId));

            var dto = mapper.Map<GetNotificationDto>(entity);

            await TryDeliverAsync(() => notificationSender.SendToUserAsync(userId, dto), $"user {userId}");
        }

        public async Task SendToGroupAsync(string groupName, CreateNotificationDto notification)
        {
            if (groupName != Roles.Admin && groupName != Roles.Provider && groupName != Roles.User)
            {
                throw new BadRequestException("Unknown notification group.");
            }

            var recipients = await userManager.GetUsersInRoleAsync(groupName);
            if (recipients.Count == 0)
            {
                logger.LogInformation("No recipients found for notification group {GroupName}.", groupName);
                return;
            }

            var entities = recipients
                .Select(u => new Notifications
                {
                    UserId = u.Id,
                    Title = notification.Title,
                    Message = notification.Message
                })
                .ToList();

            var repo = unitOfWork.Repository<Notifications>();
            foreach (var entity in entities)
            {
                await repo.AddAsync(entity);
            }
            await unitOfWork.SaveChangesAsync();

            foreach (var recipient in recipients)
            {
                await cacheService.RemoveAsync(UserCachePrefix(recipient.Id));
            }

            var groupDto = mapper.Map<GetNotificationDto>(entities.First());

            await TryDeliverAsync(() => notificationSender.SendToGroupAsync(groupName, groupDto), $"group {groupName}");
        }

        public async Task MarkAsReadAsync(Guid notificationId, string userId)
        {
            var repo = unitOfWork.Repository<Notifications>();
            var notification = await repo.GetByIdAsync(notificationId);

            if (notification is null || notification.UserId != userId)
            {
                throw new NotFoundException("Notification not found.");
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                repo.Update(notification);
                await unitOfWork.SaveChangesAsync();
                await cacheService.RemoveAsync(UserCachePrefix(userId));
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var repo = unitOfWork.Repository<Notifications>();
            var spec = new NotificationsByUserSpecification(userId, unreadOnly: true);
            var unread = await repo.GetAllAsync(spec);

            if (unread.Count == 0) return;

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                repo.Update(notification);
            }

            await unitOfWork.SaveChangesAsync();
            await cacheService.RemoveAsync(UserCachePrefix(userId));
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            var cacheKey = $"{UserCachePrefix(userId)}_UnreadCount";

            var result = await cacheService.GetOrSetAsync<int>(cacheKey, async () =>
            {
                var repo = unitOfWork.Repository<Notifications>();
                var spec = new NotificationsByUserSpecification(userId, unreadOnly: true);
                return await repo.CountAsync(spec);
            });

            return result;
        }
        private async Task TryDeliverAsync(Func<Task> deliver, string target)
        {
            try
            {
                await deliver();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to deliver real-time notification to {Target}.", target);
            }
        }
    }
}
