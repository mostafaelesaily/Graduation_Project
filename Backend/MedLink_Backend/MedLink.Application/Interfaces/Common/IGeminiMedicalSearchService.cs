using MedLink.Application.DTOs.Common.MedicalSearchDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface IGeminiMedicalSearchService
    {
        /// <summary>
        /// Calls Gemini API to extract medical search parameters and find external providers
        /// </summary>
        Task<GeminiMedicalSearchResponse> SearchExternalProvidersAsync(
            string keyword,
            double latitude,
            double longitude,
            int radiusInMeters,
            CancellationToken cancellationToken = default);
    }
}
