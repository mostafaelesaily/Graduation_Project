using MedLink.Application.DTOs.Common.ChatbotDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface IGeminiChatbotService
    {
        /// <summary>
        /// Sends the conversation history plus the new user turn (already built with
        /// USER_ROLE / AREA / PATIENT_MEDICAL_HISTORY / VERIFIED_PROVIDERS / USER MESSAGE)
        /// to Gemini, using the MedLink system prompt and knowledge files as the system instruction.
        /// </summary>
        Task<ChatbotResponseDto> GenerateReplyAsync(
            IReadOnlyList<ChatTurnDto> conversationHistory,
            string userTurnContext,
            CancellationToken cancellationToken = default);
    }
}
