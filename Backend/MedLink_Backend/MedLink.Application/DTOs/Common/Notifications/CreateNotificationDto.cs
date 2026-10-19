using MedLink.Domain.Entities.Common.Notifications;
using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.Notifications
{
    
    public class CreateNotificationDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(Notification.TitleMaxLength)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(Notification.MessageMaxLength)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [StringLength(Notification.TypeMaxLength)]
        public string Type { get; set; } = string.Empty;

        [StringLength(Notification.LinkMaxLength)]
        public string? Link { get; set; }
    }
}