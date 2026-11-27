using System.ComponentModel.DataAnnotations;

namespace MedLink.Application.DTOs.Common.ChatbotDTOs
{
    public class ChatbotRequestDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;
    }
}
