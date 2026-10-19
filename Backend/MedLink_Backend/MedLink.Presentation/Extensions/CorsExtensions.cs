namespace MedLink.Presentation.Extensions
{
    public static class CorsExtensions
    {
        public static IServiceCollection AddCQRS
            (
             this IServiceCollection services , 
             IConfiguration configuration
            )
        {
            services.AddCors(options =>
            {
                options.AddPolicy("MedLinkPolicy", policy =>
                {
                    var allowedOrigins = configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? [];

                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            return services;
        }
    }
}
