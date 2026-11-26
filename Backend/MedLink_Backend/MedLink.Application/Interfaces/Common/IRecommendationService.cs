using MedLink.Application.DTOs.Common.RecommendationDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface IRecommendationService
    {
        /// <summary>
        /// Builds provider recommendations for a patient based on their medical records
        /// (Diagnosis + Description), by extracting a search keyword via Gemini and then
        /// reusing the unified medical search (internal + external providers).
        /// </summary>
        Task<RecommendationResponseDto> GetRecommendationsAsync(
            string patientId,
            RecommendationRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
