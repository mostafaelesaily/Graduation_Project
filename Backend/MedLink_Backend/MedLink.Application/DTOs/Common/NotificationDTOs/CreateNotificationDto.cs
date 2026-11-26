using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MedLink.Application.DTOs.Common.NotificationDTOs
{
    public class CreateNotificationDto
    {
       
            [Required]
            public string UserId { get; set; }

            [Required]
            [MaxLength(100)]
            public string Title { get; set; }

            [Required]
            [MaxLength(500)]
            public string Message { get; set; }
        }
}
