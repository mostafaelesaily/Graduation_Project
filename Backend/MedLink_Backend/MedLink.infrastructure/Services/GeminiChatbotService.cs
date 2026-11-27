using MedLink.Application.DTOs.Common.ChatbotDTOs;
using MedLink.Application.Interfaces.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MedLink.infrastructure.Services
{
    public class GeminiChatbotService : IGeminiChatbotService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiChatbotService> _logger;
        private readonly HttpClient _httpClient;

        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/{0}:generateContent";

        // Loaded once per process from AI/Prompts/system-prompt.txt + every file in AI/Knowledge/,
        // the same files MedLink's AI team ships for the chat assistant (see Ai/README.md).
        private static readonly Lazy<string> SystemInstruction = new(LoadSystemInstruction);

        public GeminiChatbotService(
            IConfiguration configuration,
            ILogger<GeminiChatbotService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<ChatbotResponseDto> GenerateReplyAsync(
            IReadOnlyList<ChatTurnDto> conversationHistory,
            string userTurnContext,
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
                var temperature = _configuration.GetValue<double?>("Gemini:Features:Chatbot:Temperature") ?? 0.2;
                var maxOutputTokens = _configuration.GetValue<int?>("Gemini:Features:Chatbot:MaxOutputTokens") ?? 2048;

                var contents = conversationHistory
                    .Select(turn => new { role = turn.Role, parts = new[] { new { text = turn.Text } } })
                    .Append(new { role = "user", parts = new[] { new { text = userTurnContext } } })
                    .ToArray();

                var requestPayload = new
                {
                    system_instruction = new { parts = new[] { new { text = SystemInstruction.Value } } },
                    contents,
                    generationConfig = new
                    {
                        temperature,
                        maxOutputTokens,
                        responseMimeType = "application/json"
                    }
                };

                var url = string.Format(GEMINI_API_URL, model) + $"?key={apiKey}";
                var jsonContent = JsonSerializer.Serialize(requestPayload);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Calling Gemini API for chatbot reply, Model: {Model}", model);

                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API returned {StatusCode}: {ErrorBody}", (int)response.StatusCode, errorBody);
                    throw new HttpRequestException($"Gemini API error {(int)response.StatusCode}: {errorBody}");
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
                                    _logger.LogWarning("Gemini returned empty chatbot response text");
                                    return FallbackResponse();
                                }

                                var chatResponse = JsonSerializer.Deserialize<ChatbotResponseDto>(responseText, options);

                                if (chatResponse == null)
                                {
                                    _logger.LogWarning("Failed to deserialize chatbot response - Invalid JSON format");
                                    return FallbackResponse();
                                }

                                _logger.LogInformation("Chatbot reply completed - Scope: {Scope}", chatResponse.Scope);

                                return chatResponse;
                            }
                        }
                    }
                }

                _logger.LogWarning("Unexpected Gemini response structure for chatbot");
                return FallbackResponse();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error when calling Gemini API for chatbot");
                throw;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Chatbot request was cancelled");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API for chatbot");
                throw;
            }
        }

        private static ChatbotResponseDto FallbackResponse() => new()
        {
            Scope = "medlink_help",
            Language = "en",
            Reply = "Sorry, I couldn't process that right now. Please try again."
        };

        private static string LoadSystemInstruction()
        {
            var baseDir = AppContext.BaseDirectory;
            var promptPath = Path.Combine(baseDir, "AI", "Prompts", "system-prompt.txt");
            var knowledgeDir = Path.Combine(baseDir, "AI", "Knowledge");

            var sb = new StringBuilder();
            sb.Append(File.Exists(promptPath) ? File.ReadAllText(promptPath) : string.Empty);

            if (Directory.Exists(knowledgeDir))
            {
                foreach (var file in Directory.GetFiles(knowledgeDir).OrderBy(f => f))
                {
                    var fileName = Path.GetFileName(file);
                    sb.AppendLine();
                    sb.AppendLine($"<<<KNOWLEDGE FILE: {fileName}>>>");
                    sb.Append(File.ReadAllText(file));
                    sb.AppendLine();
                    sb.AppendLine($"<<<END OF {fileName}>>>");
                }
            }

            return sb.ToString();
        }
    }
}
