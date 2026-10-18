
using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services.Common;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class Notification_Dependency_Injection
    {
        public static IServiceCollection AddNotificationService(
            this IServiceCollection services)
        {
            services.AddScoped<
                INotificationService,
                NotificationService>();

            return services;
        }
    }
}
