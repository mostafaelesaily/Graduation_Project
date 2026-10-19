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
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            return services;
        }
    }
}
