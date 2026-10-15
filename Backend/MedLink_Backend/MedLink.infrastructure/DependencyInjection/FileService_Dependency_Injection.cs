using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class FileService_Dependency_Injection
    {
        public static IServiceCollection AddFileService(this IServiceCollection services)
        {
            services.AddScoped<IFileService, FileService>();
            return services;
        }
    }
}
