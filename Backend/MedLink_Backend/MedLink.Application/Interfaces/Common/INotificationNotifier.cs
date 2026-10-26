using MedLink.Application.DTOs.Common.NotificationDTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MedLink.Application.Interfaces.Common
{
    public interface INotificationNotifier
    {
        Task NotifyUserAsync(string userId, NotificationDto notification, CancellationToken cancellationToken = default);
    }
}