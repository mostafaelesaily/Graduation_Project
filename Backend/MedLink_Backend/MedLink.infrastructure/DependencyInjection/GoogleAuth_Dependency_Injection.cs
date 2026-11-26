using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class GoogleAuth_Dependency_Injection
    {
        public static IServiceCollection AddGoogleAuthService(this IServiceCollection services)
        {
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            return services;
        }
    }
}
