using MedLink.Application.Interfaces.Common;
using MedLink.Application.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.Application.Dependency_Injection
{
    public static class Chatbot_Dependency_Injection
    {
        public static IServiceCollection AddChatbotDI(this IServiceCollection services)
        {
            services.AddScoped<IChatbotService, ChatbotService>();

            return services;
        }
    }
}
