using MedLink.Application.Interfaces.Common;
using MedLink.infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedLink.infrastructure.DependencyInjection
{
    public static class Persistence_Dependency_Injection
    {
        public static IServiceCollection AddPersistence
            (
            this IServiceCollection services,
            IConfiguration configuration
            )
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("MedLink_DB"));
            });

            services.AddScoped<IUnitOfWork, MedLink.infrastructure.UnitOfWork.UnitOfWork>();

            return services;
        }
    }
}