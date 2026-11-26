using MedLink.Application.Interfaces.Common;
using MedLink.Application.Mapping;
using MedLink.Application.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.Application.Dependency_Injection
{
    public static class MedicalSearch_Dependency_Injection
    {
        public static IServiceCollection AddMedicalSearchDI(this IServiceCollection services)
        {
            // Register services
            services.AddScoped<IMedicalSearchService, MedicalSearchService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            return services;
        }
    }
}
