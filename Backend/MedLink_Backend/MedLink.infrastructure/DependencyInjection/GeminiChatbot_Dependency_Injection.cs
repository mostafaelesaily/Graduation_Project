using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class GeminiChatbot_Dependency_Injection
    {
        public static IServiceCollection AddGeminiChatbotService(this IServiceCollection services)
        {
            services.AddHttpClient<IGeminiChatbotService, GeminiChatbotService>()
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

            return services;
        }
    }
}
