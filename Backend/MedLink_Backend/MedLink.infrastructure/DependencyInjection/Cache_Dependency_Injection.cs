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
                var redisOptions =
                    ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"));

                // Fail fast and keep retrying in the background when Redis is down.
                redisOptions.AbortOnConnectFail = false;
                redisOptions.ConnectTimeout = 1000;
                redisOptions.SyncTimeout = 1000;

                options.ConfigurationOptions = redisOptions;
                options.InstanceName = "RedisCacheInstance";
            });
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var connectionString = configuration.GetConnectionString("Redis");
                var options = ConfigurationOptions.Parse(connectionString);

                // Do not crash the application when Redis is not running; the
                // cache simply keeps retrying in the background.
                options.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(options);
            });
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}
