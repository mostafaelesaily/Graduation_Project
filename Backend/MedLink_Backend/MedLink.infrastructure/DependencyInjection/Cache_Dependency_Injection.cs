using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class Cache_Dependency_Injection
    {
        public static IServiceCollection AddCacheService(this IServiceCollection services
            , IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    configuration.GetConnectionString("Redis");

                options.InstanceName = "RedisCacheInstance";
            });
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(connectionString);
            });
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
