using MedLink.Application.Interfaces.Common;
using MedLink.Application.Mapping;
using MedLink.Application.Services.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.Application.Dependency_Injection
{
    public static class Dependency_Injection
    {
        public static IServiceCollection AddAppDI( this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { },
                  typeof(UserMappingProfile).Assembly
                  );
            services.AddScoped<IAccountService, AccountService>();
            return services;
        }
    }
}
