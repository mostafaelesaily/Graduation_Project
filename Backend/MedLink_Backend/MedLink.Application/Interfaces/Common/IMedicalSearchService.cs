using MedLink.Application.DTOs.Common.MedicalSearchDTOs;

namespace MedLink.Application.Interfaces.Common
{
    public interface IMedicalSearchService
    {
        /// <summary>
        /// Performs unified medical search - searches both internal MedLink providers and external providers
        /// </summary>
        Task<UnifiedMedicalSearchResponseDto> SearchAsync(
            MedicalSearchRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
