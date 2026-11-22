using System.Text.Json.Serialization;

namespace MedLink.Application.DTOs.Common.MedicalSearchDTOs
{
    public class GeminiMedicalSearchResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("query_info")]
        public QueryInfo QueryInfo { get; set; }

        [JsonPropertyName("results")]
        public List<GeminiPlaceResult> Results { get; set; } = new();

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class QueryInfo
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }
    }

    public class GeminiPlaceResult
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("type_and_specialty")]
        public string TypeAndSpecialty { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("phone")]
        public string Phone { get; set; }

        [JsonPropertyName("working_hours")]
        public string WorkingHours { get; set; }

        [JsonPropertyName("google_maps_url")]
        public string GoogleMapsUrl { get; set; }
    }
}
