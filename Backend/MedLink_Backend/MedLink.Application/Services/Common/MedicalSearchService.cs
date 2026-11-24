using AutoMapper;
using MedLink.Application.DTOs.Common.MedicalSearchDTOs;
using MedLink.Application.DTOs.Common.ProviderDTOs;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.ProviderProfiles;
using MedLink.Domain.Entities.Provider;
using Microsoft.Extensions.Logging;

namespace MedLink.Application.Services.Common
{
    public class MedicalSearchService : IMedicalSearchService
    {
        private readonly IGeminiMedicalSearchService _geminiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicalSearchService> _logger;

        public MedicalSearchService(
            IGeminiMedicalSearchService geminiService,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<MedicalSearchService> logger)
        {
            _geminiService = geminiService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UnifiedMedicalSearchResponseDto> SearchAsync(
            MedicalSearchRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            ValidateSearchRequest(request);

            _logger.LogInformation(
                "Unified medical search initiated - Keyword: {Keyword}, Location: {Lat},{Lon}, Radius: {Radius}m",
                request.Keyword, request.Latitude, request.Longitude, request.RadiusInMeters);

            var metadata = new SearchMetadataDto();
            var internalProviders = new InternalProvidersResultDto();
            var externalProviders = new ExternalProvidersResultDto();

            // Execute both searches concurrently
            var internalTask = SearchInternalProvidersAsync(request, metadata, cancellationToken);
            var externalTask = SearchExternalProvidersAsync(request, metadata, cancellationToken);

            await Task.WhenAll(internalTask, externalTask);

            internalProviders = await internalTask;
            externalProviders = await externalTask;

            metadata.InternalCount = internalProviders.TotalCount;
            metadata.ExternalCount = externalProviders.TotalCount;
            metadata.TotalCount = internalProviders.TotalCount + externalProviders.TotalCount;

            var response = new UnifiedMedicalSearchResponseDto
            {
                SearchQuery = request.Keyword,
                Results = new SearchResultsDto
                {
                    MedlinkProviders = internalProviders,
                    ExternalProviders = externalProviders
                },
                Metadata = metadata
            };

            _logger.LogInformation(
                "Unified search completed - Internal: {InternalCount}, External: {ExternalCount}, Total: {TotalCount}",
                metadata.InternalCount, metadata.ExternalCount, metadata.TotalCount);

            return response;
        }

        private async Task<InternalProvidersResultDto> SearchInternalProvidersAsync(
            MedicalSearchRequestDto request,
            SearchMetadataDto metadata,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting internal provider search");

                // Create specification WITHOUT pagination first
                var specification = new MedicalSearchSpecification(
                    request.Keyword,
                    request.Latitude,
                    request.Longitude,
                    request.RadiusInMeters,
                    1,  // Page 1
                    int.MaxValue);  // Get all results

                // Get all providers matching the search criteria
                var allProviders = await _unitOfWork
                    .Repository<ProviderProfile>()
                    .GetAllAsync(specification);

                _logger.LogInformation("DEBUG: Specification returned {Count} providers", allProviders.Count);
                foreach (var p in allProviders)
                {
                    _logger.LogInformation("DEBUG: Provider {Id} - Status: {Status}, Services: {ServiceCount}",
                        p.Id, p.verificationStatus, p.ProviderServices.Count);
                }

                // Filter by radius in-memory
                var providersInRadius = allProviders
                    .Where(p => specification.IsWithinRadius(p))
                    .ToList();

                _logger.LogInformation("DEBUG: After radius filter: {Count} providers", providersInRadius.Count);

                var totalCount = providersInRadius.Count;

                // Apply pagination to filtered results
                var paginatedProviders = providersInRadius
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Map to DTOs
                var providerDtos = _mapper.Map<List<GetProviderResultDto>>(paginatedProviders);

                _logger.LogInformation(
                    "Internal provider search completed - Total: {Total}, Returned: {Returned}",
                    totalCount, paginatedProviders.Count);

                return new InternalProvidersResultDto
                {
                    Items = providerDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching internal providers");
                metadata.InternalSearchSucceeded = false;
                metadata.InternalSearchError = ex.Message;

                return new InternalProvidersResultDto
                {
                    Items = new(),
                    TotalCount = 0,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };
            }
        }

        private async Task<ExternalProvidersResultDto> SearchExternalProvidersAsync(
            MedicalSearchRequestDto request,
            SearchMetadataDto metadata,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Starting external provider search via Gemini");

                var geminiResponse = await _geminiService.SearchExternalProvidersAsync(
                    request.Keyword,
                    request.Latitude,
                    request.Longitude,
                    request.RadiusInMeters,
                    cancellationToken);

                if (geminiResponse == null || geminiResponse.Status != "success")
                {
                    _logger.LogWarning("Gemini search status: {Status}", geminiResponse?.Status);
                    return new ExternalProvidersResultDto
                    {
                        Items = new(),
                        TotalCount = 0
                    };
                }

                // Map Gemini results to ExternalProviderDto
                var externalProviders = geminiResponse.Results?
                    .Select(result => new ExternalProviderDto
                    {
                        Name = result.Name,
                        Category = result.Category,
                        Type = result.TypeAndSpecialty,
                        Address = result.Address,
                        Phone = result.Phone,
                        WorkingHours = result.WorkingHours,
                        GoogleMapsUrl = result.GoogleMapsUrl,
                        Source = "Gemini"
                    })
                    .ToList() ?? new();

                _logger.LogInformation("External provider search completed - Found: {Count}", externalProviders.Count);

                return new ExternalProvidersResultDto
                {
                    Items = externalProviders,
                    TotalCount = externalProviders.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching external providers");
                metadata.ExternalSearchSucceeded = false;
                metadata.ExternalSearchError = ex.Message;

                return new ExternalProvidersResultDto
                {
                    Items = new(),
                    TotalCount = 0
                };
            }
        }

        private void ValidateSearchRequest(MedicalSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Keyword))
                throw new ArgumentException("Keyword cannot be empty", nameof(request.Keyword));

            if (request.Latitude < -90 || request.Latitude > 90)
                throw new ArgumentException("Invalid latitude. Must be between -90 and 90", nameof(request.Latitude));

            if (request.Longitude < -180 || request.Longitude > 180)
                throw new ArgumentException("Invalid longitude. Must be between -180 and 180", nameof(request.Longitude));

            if (request.RadiusInMeters <= 0)
                throw new ArgumentException("Radius must be greater than 0", nameof(request.RadiusInMeters));

            if (request.PageNumber < 1)
                throw new ArgumentException("Page number must be at least 1", nameof(request.PageNumber));

            if (request.PageSize < 1)
                throw new ArgumentException("Page size must be at least 1", nameof(request.PageSize));
        }
    }
}
