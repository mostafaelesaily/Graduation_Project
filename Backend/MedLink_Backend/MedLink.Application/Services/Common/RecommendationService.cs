using MedLink.Application.DTOs.Common.MedicalSearchDTOs;
using MedLink.Application.DTOs.Common.RecommendationDTOs;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.MedicalRecords;
using MedLink.Domain.Entities.Medical_History;
using Microsoft.Extensions.Logging;

namespace MedLink.Application.Services.Common
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IGeminiMedicalSearchService _geminiService;
        private readonly IMedicalSearchService _medicalSearchService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RecommendationService> _logger;

        public RecommendationService(
            IGeminiMedicalSearchService geminiService,
            IMedicalSearchService medicalSearchService,
            IUnitOfWork unitOfWork,
            ILogger<RecommendationService> logger)
        {
            _geminiService = geminiService;
            _medicalSearchService = medicalSearchService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<RecommendationResponseDto> GetRecommendationsAsync(
            string patientId,
            RecommendationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient id is required", nameof(patientId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var records = await _unitOfWork
                .Repository<MedicalRecord>()
                .GetAllAsync(new PatientMedicalRecordsSpecification(patientId));

            if (records.Count == 0)
            {
                _logger.LogInformation("No medical records found for patient {PatientId}, cannot build recommendations", patientId);

                return new RecommendationResponseDto
                {
                    ExtractedKeyword = null,
                    BasedOnRecordsCount = 0,
                    Results = new SearchResultsDto
                    {
                        MedlinkProviders = new InternalProvidersResultDto(),
                        ExternalProviders = new ExternalProvidersResultDto()
                    },
                    Metadata = new SearchMetadataDto
                    {
                        InternalSearchSucceeded = false,
                        InternalSearchError = "No medical records found for this patient",
                        ExternalSearchSucceeded = false,
                        ExternalSearchError = "No medical records found for this patient"
                    }
                };
            }

            var medicalContext = string.Join(
                ". ",
                records.Select(r => $"Diagnosis: {r.Diagnosis}. Description: {r.Description}"));

            _logger.LogInformation(
                "Building recommendations for patient {PatientId} from {Count} medical records",
                patientId, records.Count);

            var keyword = await _geminiService.ExtractKeywordAsync(medicalContext, cancellationToken);

            if (string.IsNullOrWhiteSpace(keyword))
            {
                _logger.LogWarning("Gemini could not extract a keyword for patient {PatientId}", patientId);

                return new RecommendationResponseDto
                {
                    ExtractedKeyword = null,
                    BasedOnRecordsCount = records.Count,
                    Results = new SearchResultsDto
                    {
                        MedlinkProviders = new InternalProvidersResultDto(),
                        ExternalProviders = new ExternalProvidersResultDto()
                    },
                    Metadata = new SearchMetadataDto
                    {
                        InternalSearchSucceeded = false,
                        InternalSearchError = "Could not extract a search keyword from medical records",
                        ExternalSearchSucceeded = false,
                        ExternalSearchError = "Could not extract a search keyword from medical records"
                    }
                };
            }

            var searchRequest = new MedicalSearchRequestDto
            {
                Keyword = keyword,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusInMeters = request.RadiusInMeters,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var searchResult = await _medicalSearchService.SearchAsync(searchRequest, cancellationToken);

            return new RecommendationResponseDto
            {
                ExtractedKeyword = keyword,
                BasedOnRecordsCount = records.Count,
                Results = searchResult.Results,
                Metadata = searchResult.Metadata
            };
        }
    }
}
