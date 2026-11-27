namespace MedLink.Application.DTOs.Common.ChatbotDTOs
{
    /// <summary>
    /// One turn (user or model) kept in the short-lived conversation memory (Redis, 2 hours).
    /// </summary>
    public class ChatTurnDto
    {
        public string Role { get; set; } = "user";
        public string Text { get; set; } = string.Empty;
    }
}
