using System.Text.Json.Serialization;

namespace MedLink.Application.DTOs.Common.MedicalSearchDTOs
{
    public class GeminiKeywordExtractionResponse
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }
    }
}
