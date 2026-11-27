using System.Text.Json.Serialization;

namespace MedLink.Application.DTOs.Common.ChatbotDTOs
{
    public class ChatbotResponseDto
    {
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("reply")]
        public string Reply { get; set; } = string.Empty;

        [JsonPropertyName("specialty")]
        public string? Specialty { get; set; }

        [JsonPropertyName("provider_type")]
        public string? ProviderType { get; set; }

        [JsonPropertyName("urgency")]
        public string? Urgency { get; set; }

        [JsonPropertyName("suggested_action")]
        public ChatbotSuggestedActionDto? SuggestedAction { get; set; }

        [JsonPropertyName("disclaimer")]
        public string? Disclaimer { get; set; }
    }

    public class ChatbotSuggestedActionDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("screen")]
        public string Screen { get; set; } = string.Empty;
    }
}
