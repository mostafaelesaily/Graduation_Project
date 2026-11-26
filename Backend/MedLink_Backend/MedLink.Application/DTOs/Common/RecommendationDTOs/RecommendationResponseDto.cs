using MedLink.Application.DTOs.Common.MedicalSearchDTOs;

namespace MedLink.Application.DTOs.Common.RecommendationDTOs
{
    public class RecommendationResponseDto
    {
        // The keyword Gemini extracted from the patient's medical records
        // (Diagnosis + Description), used to drive both searches.
        public string ExtractedKeyword { get; set; }
        public int BasedOnRecordsCount { get; set; }
        public SearchResultsDto Results { get; set; }
        public SearchMetadataDto Metadata { get; set; }
    }
}
