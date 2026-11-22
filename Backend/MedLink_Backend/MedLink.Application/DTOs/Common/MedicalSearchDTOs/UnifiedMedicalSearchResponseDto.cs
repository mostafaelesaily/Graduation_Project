namespace MedLink.Application.DTOs.Common.MedicalSearchDTOs
{
    public class UnifiedMedicalSearchResponseDto
    {
        public string SearchQuery { get; set; }
        public SearchResultsDto Results { get; set; }
        public SearchMetadataDto Metadata { get; set; }
    }

    public class SearchResultsDto
    {
        public InternalProvidersResultDto MedlinkProviders { get; set; }
        public ExternalProvidersResultDto ExternalProviders { get; set; }
    }

    public class InternalProvidersResultDto
    {
        public List<GetProviderResultDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
        public bool HasNext => PageNumber < TotalPages;
        public bool HasPrevious => PageNumber > 1;
    }

    public class ExternalProvidersResultDto
    {
        public List<ExternalProviderDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class GetProviderResultDto
    {
        public Guid ProviderId { get; set; }
        public string ServiceName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SearchMetadataDto
    {
        public int InternalCount { get; set; }
        public int ExternalCount { get; set; }
        public int TotalCount { get; set; }
        public bool InternalSearchSucceeded { get; set; } = true;
        public bool ExternalSearchSucceeded { get; set; } = true;
        public string InternalSearchError { get; set; }
        public string ExternalSearchError { get; set; }
    }
}
