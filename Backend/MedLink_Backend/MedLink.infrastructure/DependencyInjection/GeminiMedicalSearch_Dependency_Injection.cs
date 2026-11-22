using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class GeminiMedicalSearch_Dependency_Injection
    {
        public static IServiceCollection AddGeminiMedicalSearchService(this IServiceCollection services)
        {
            services.AddHttpClient<IGeminiMedicalSearchService, GeminiMedicalSearchService>()
                .ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                });

            return services;
        }
    }
}
