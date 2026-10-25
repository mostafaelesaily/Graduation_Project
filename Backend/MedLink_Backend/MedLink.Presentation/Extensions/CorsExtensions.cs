namespace MedLink.Presentation.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", policy =>
                {
                    var allowedOrigins = configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>();

                    policy
                        .WithOrigins(allowedOrigins ?? Array.Empty<string>())
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}

