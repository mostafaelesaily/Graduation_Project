using MedLink.Domain.Entities.Common.Identity;
using MedLink.infrastructure.Data;
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

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await MedicalSearchTestSeeder.SeedAsync(userManager, context);
            await DatabaseSeeder.SeedAsync(userManager, context);
        }
    }
}
