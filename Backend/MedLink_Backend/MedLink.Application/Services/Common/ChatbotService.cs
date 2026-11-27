using MedLink.Application.DTOs.Common.ChatbotDTOs;
using MedLink.Application.Interfaces.Common;
using MedLink.Application.Specifications.MedicalRecords;
using MedLink.Domain.Entities.Medical_History;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MedLink.Application.Services.Common
{
    public class ChatbotService : IChatbotService
    {
        private readonly IGeminiChatbotService _geminiChatbotService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatbotService> _logger;

        private const int MaxMedicalRecords = 10;
        private const int MedicalRecordsLookbackYears = 3;
        private const int MaxConversationTurns = 20; // last 10 exchanges
        private static readonly TimeSpan ConversationTtl = TimeSpan.FromHours(2);

        public ChatbotService(
            IGeminiChatbotService geminiChatbotService,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IConfiguration configuration,
            ILogger<ChatbotService> logger)
        {
            _geminiChatbotService = geminiChatbotService;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ChatbotResponseDto> SendMessageAsync(
            string patientId,
            ChatbotRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient id is required", nameof(patientId));

            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message is required", nameof(request));

            if (!(_configuration.GetValue<bool?>("Gemini:Features:Chatbot:Enabled") ?? true))
                throw new InvalidOperationException("The MedLink AI assistant is currently disabled.");

            var historyKey = ConversationKey(patientId);
            var history = await _cacheService.GetAsync<List<ChatTurnDto>>(historyKey) ?? new List<ChatTurnDto>();

            var medicalHistoryBlock = await BuildMedicalHistoryBlockAsync(patientId, cancellationToken);

            var userTurnContext = BuildUserTurnContext(request, medicalHistoryBlock);

            _logger.LogInformation("Sending chatbot message for patient {PatientId}", patientId);

            var reply = await _geminiChatbotService.GenerateReplyAsync(history, userTurnContext, cancellationToken);

            history.Add(new ChatTurnDto { Role = "user", Text = userTurnContext });
            history.Add(new ChatTurnDto { Role = "model", Text = JsonSerializer.Serialize(reply) });

            if (history.Count > MaxConversationTurns)
                history = history.Skip(history.Count - MaxConversationTurns).ToList();

            await _cacheService.setAsync(historyKey, history, absoluteExpiration: ConversationTtl);

            return reply;
        }

        public async Task ResetConversationAsync(string patientId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("Patient id is required", nameof(patientId));

            await _cacheService.RemoveAsync(ConversationKey(patientId));
        }

        private static string ConversationKey(string patientId) => $"chatbot:history:{patientId}";

        private static string BuildUserTurnContext(ChatbotRequestDto request, string medicalHistoryBlock)
        {
            var sb = new StringBuilder();
            sb.AppendLine("USER_ROLE: patient");
            sb.AppendLine("PATIENT_MEDICAL_HISTORY:");
            sb.AppendLine(medicalHistoryBlock);
            sb.AppendLine("USER MESSAGE:");
            sb.Append(request.Message.Trim());

            return sb.ToString();
        }

        private async Task<string> BuildMedicalHistoryBlockAsync(string patientId, CancellationToken cancellationToken)
        {
            var records = await _unitOfWork
                .Repository<MedicalRecord>()
                .GetAllAsync(new PatientMedicalRecordsSpecification(patientId));

            var cutoff = DateTime.UtcNow.AddYears(-MedicalRecordsLookbackYears);

            var recent = records
                .Where(r => r.RecordDate >= cutoff)
                .Take(MaxMedicalRecords)
                .ToList();

            if (recent.Count == 0)
                return "none recorded";

            return string.Join(
                Environment.NewLine,
                recent.Select(r => $"* {r.RecordDate:yyyy-MM-dd} | {r.Title} | diagnosis: {r.Diagnosis}"));
        }
    }
}
