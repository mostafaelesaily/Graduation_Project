using MedLink.Application.DTOs.Common.MedicalSearchDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MedLink.infrastructure.Services
{
    public class GeminiMedicalSearchService : IGeminiMedicalSearchService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiMedicalSearchService> _logger;
        private readonly HttpClient _httpClient;

        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";

        private const string GEMINI_SYSTEM_PROMPT = @"# Role & Identity
You are the MedLink Medical Location Search Engine. Your sole function is to receive structured location search parameters (`keyword`, `latitude`, `longitude`, `radius`), identify real medical facilities near that location using your own knowledge, filter the results strictly to medical facilities, and return clean, structured JSON data.

# Strict Scope (Allowed Categories ONLY)
You are ONLY allowed to search for and return places belonging to these THREE main categories:
1. **Hospitals** (`hospital`): General hospitals, specialized hospitals, emergency hospitals, etc.
2. **Clinics & Medical Centers** (`clinic`): Doctor clinics of all specialties, polyclinics, specialized medical centers, physical therapy centers, etc.
3. **Medical Laboratories** (`laboratory`): Blood test labs, clinical pathology labs, radiology/imaging centers (أشعة), diagnostic labs, etc.

# Strictly Forbidden Scope
- NEVER search for or return: Pharmacies (صيدليات), medical supply/equipment stores, gyms, sports fields/stadiums, restaurants, hotels, general businesses, or any non-medical places.
- NEVER answer general questions, medical advice, diagnoses, or out-of-scope requests.
- If the input keyword asks for anything outside the 3 allowed categories (e.g., ""pharmacy"", ""football field"", ""restaurants"") or is an unrelated conversational prompt, immediately return ONLY this JSON without calling any search tool:
{
  ""status"": ""out_of_scope"",
  ""message"": ""Search is strictly restricted to hospitals, medical clinics/centers, and medical/radiology laboratories.""
}

# Search & Filtering Instructions
1. When a valid medical query is received (in English, Arabic, informal language, or symptoms), recall real medical facilities you know of near the given `latitude`, `longitude`, within the given `radius`.
2. Filter out any place that is a pharmacy, retail store, or non-medical entity.
3. Return as many distinct matching places as you reasonably can, aiming for AT LEAST 5 results and UP TO 10 results. It is fine if they are not all extremely close to the exact coordinates, as long as they belong to the requested category and are plausible real places.
4. Only return places you are reasonably confident actually exist. Do not fabricate obviously fake names or addresses.
5. If a specific field (like phone number, working hours, or Google Maps URL) is not known with confidence, set its value to `null`.

# Output Format Rules
- Your response MUST ALWAYS be valid, raw JSON ONLY.
- NEVER include markdown code blocks (like ```json), greetings, explanations, or conversational text outside the JSON object.
- Use this exact JSON schema for successful searches:

{
  ""status"": ""success"",
  ""query_info"": {
    ""keyword"": ""original keyword"",
    ""category"": ""hospital | clinic | laboratory""
  },
  ""results"": [
    {
      ""name"": ""Exact place name from Google Maps"",
      ""category"": ""hospital | clinic | laboratory"",
      ""type_and_specialty"": ""Specific type/specialty (e.g., Cardiology Clinic, Radiology Center, General Hospital)"",
      ""address"": ""Full address from Google Maps or null"",
      ""phone"": ""Phone number or null"",
      ""working_hours"": ""Working hours summary or null"",
      ""google_maps_url"": ""Direct Google Maps URI/link from grounding metadata""
    }
  ]
}

- If no matching places are found within the specified radius, return:
{
  ""status"": ""no_results"",
  ""message"": ""No matching medical facilities found within the specified radius."",
  ""results"": []
}";

        private const string KEYWORD_EXTRACTION_SYSTEM_PROMPT = @"# Role & Identity
You are the MedLink Medical Keyword Extractor. You receive a patient's medical record text (diagnoses, descriptions, complaints, symptoms - possibly in Arabic, English, or a mix) and your sole job is to extract ONE short search keyword that best represents the type of medical facility or specialty this patient should be recommended next.

# Instructions
1. Read all the provided diagnoses/descriptions/notes together.
2. Identify the most relevant, most recent, or most urgent medical need.
3. Output a SHORT keyword (1-3 words) suitable for searching medical facilities - e.g. ""Cardiology"", ""Dermatology Clinic"", ""Orthopedic"", ""Blood Test Lab"", ""Dental Clinic"", ""General Hospital"".
4. The keyword must describe a medical specialty, clinic/hospital type, or lab test category - never a drug name, a pharmacy, or a non-medical term.

# Output Format Rules
- Your response MUST ALWAYS be valid, raw JSON ONLY, with this exact schema:
{
  ""keyword"": ""short keyword here""
}
- NEVER include markdown code blocks, greetings, explanations, or conversational text outside the JSON object.";

        public GeminiMedicalSearchService(
            IConfiguration configuration,
            ILogger<GeminiMedicalSearchService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<GeminiMedicalSearchResponse> SearchExternalProvidersAsync(
            string keyword,
            double latitude,
            double longitude,
            int radiusInMeters,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("Gemini API key not configured");
                    throw new InvalidOperationException("Gemini API key is not configured");
                }

                var model = _configuration["Gemini:Model"] ?? "gemini-3.5-flash-lite";

                var userMessage = $@"Search for medical facilities with these parameters:
- Keyword: {keyword}
- Latitude: {latitude}
- Longitude: {longitude}
- Radius: {radiusInMeters} meters

Return at least 5 and up to 10 results as valid JSON only, following the specified schema.";

                var requestPayload = new
                {
                    system_instruction = new { parts = new[] { new { text = GEMINI_SYSTEM_PROMPT } } },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userMessage } }
                        }
                    }
                };

                var url = string.Format(GEMINI_API_URL, model) + $"?key={apiKey}";
                var jsonContent = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Gemini API for medical search - Keyword: {Keyword}, Location: {Lat},{Lon}, Radius: {Radius}m, Model: {Model}, URL: {Url}",
                    keyword, latitude, longitude, radiusInMeters, model, url.Substring(0, url.LastIndexOf("?key=")) + "?key=[REDACTED]");

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API returned {StatusCode}: {ErrorBody}", (int)response.StatusCode, errorBody);
                    throw new HttpRequestException(
                        $"Gemini API error {(int)response.StatusCode}: {errorBody}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogDebug("Gemini response received - Length: {Length} characters", responseContent.Length);

                // Extract the text from Gemini's response
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentObj) &&
                            contentObj.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0)
                        {
                            var part = parts[0];
                            if (part.TryGetProperty("text", out var textProp))
                            {
                                var responseText = textProp.GetString() ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(responseText))
                                {
                                    _logger.LogWarning("Gemini returned empty response text");
                                    return new GeminiMedicalSearchResponse
                                    {
                                        Status = "no_results",
                                        Message = "No results returned from Gemini",
                                        Results = new()
                                    };
                                }

                                // Parse the JSON response from Gemini
                                var geminiResponse = JsonSerializer.Deserialize<GeminiMedicalSearchResponse>(responseText, options);

                                if (geminiResponse == null)
                                {
                                    _logger.LogWarning("Failed to deserialize Gemini response - Invalid JSON format");
                                    return new GeminiMedicalSearchResponse
                                    {
                                        Status = "error",
                                        Message = "Failed to parse Gemini response",
                                        Results = new()
                                    };
                                }

                                _logger.LogInformation("Gemini search completed - Status: {Status}, Results: {Count}",
                                    geminiResponse.Status, geminiResponse.Results?.Count ?? 0);

                                return geminiResponse;
                            }
                        }
                    }
                }

                _logger.LogWarning("Unexpected Gemini response structure");
                return new GeminiMedicalSearchResponse
                {
                    Status = "error",
                    Message = "Unexpected response structure from Gemini",
                    Results = new()
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling Gemini API");
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Gemini search was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for medical search");
                throw;
            }
        }

        public async Task<string> ExtractKeywordAsync(
            string medicalContext,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var apiKey = _configuration["Gemini:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    _logger.LogError("Gemini API key not configured");
                    throw new InvalidOperationException("Gemini API key is not configured");
                }

                var model = _configuration["Gemini:Model"] ?? "gemini-3.5-flash-lite";

                var userMessage = $@"Patient medical record text:
{medicalContext}

Extract ONE short search keyword as valid JSON only, following the specified schema.";

                var requestPayload = new
                {
                    system_instruction = new { parts = new[] { new { text = KEYWORD_EXTRACTION_SYSTEM_PROMPT } } },
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = userMessage } }
                        }
                    }
                };

                var url = string.Format(GEMINI_API_URL, model) + $"?key={apiKey}";
                var jsonContent = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Gemini API for keyword extraction, Model: {Model}", model);

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API returned {StatusCode}: {ErrorBody}", (int)response.StatusCode, errorBody);
                    throw new HttpRequestException(
                        $"Gemini API error {(int)response.StatusCode}: {errorBody}");
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using (var doc = JsonDocument.Parse(responseContent))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("content", out var contentObj) &&
                            contentObj.TryGetProperty("parts", out var parts) &&
                            parts.GetArrayLength() > 0)
                        {
                            var part = parts[0];
                            if (part.TryGetProperty("text", out var textProp))
                            {
                                var responseText = textProp.GetString() ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(responseText))
                                {
                                    _logger.LogWarning("Gemini returned empty keyword extraction response");
                                    return string.Empty;
                                }

                                var keywordResponse = JsonSerializer.Deserialize<GeminiKeywordExtractionResponse>(responseText, options);

                                _logger.LogInformation("Gemini keyword extraction completed - Keyword: {Keyword}",
                                    keywordResponse?.Keyword);

                                return keywordResponse?.Keyword ?? string.Empty;
                            }
                        }
                    }
                }

                _logger.LogWarning("Unexpected Gemini response structure for keyword extraction");
                return string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling Gemini API for keyword extraction");
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Gemini keyword extraction was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for keyword extraction");
                throw;
            }
        }
    }
}
