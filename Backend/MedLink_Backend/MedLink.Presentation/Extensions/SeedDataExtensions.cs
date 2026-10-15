using MedLink.infrastructure.Seed;
using Microsoft.AspNetCore.Identity;

namespace MedLink.Presentation.Extensions
{
    public static class SeedDataExtensions
    {
        public static async Task SeedDatabaseAsync(this WebApplication application)
        {
            using var scope = application.Services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await RoleSeeder.SeedRoleAsync(roleManager);
        }
    }
}
