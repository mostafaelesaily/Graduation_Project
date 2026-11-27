using MedLink.Application.DTOs.Common.ChatbotDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface IChatbotService
    {
        /// <summary>
        /// Sends a patient message to the MedLink AI health assistant, attaching the
        /// patient's own medical history and (when a location is given) nearby verified
        /// providers, and keeps a short-lived conversation memory (2 hours) in Redis.
        /// </summary>
        Task<ChatbotResponseDto> SendMessageAsync(
            string patientId,
            ChatbotRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears the patient's current conversation memory (starts a new chat).
        /// </summary>
        Task ResetConversationAsync(string patientId, CancellationToken cancellationToken = default);
    }
}
