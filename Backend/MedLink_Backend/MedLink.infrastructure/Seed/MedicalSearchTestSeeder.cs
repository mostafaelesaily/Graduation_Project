using MedLink.Application.Constants;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using MedLink.infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedLink.infrastructure.Seed
{
    public static class MedicalSearchTestSeeder
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager, AppDbContext context)
        {
            await SeedProviderAsync(
                userManager, context,
                email: "seed.clinic@medlink.test",
                userName: "seed_clinic",
                serviceName: "Clinic",
                description: "General medical clinic for testing search",
                providerType: ProviderType.Clinic,
                latitude: 30.0444,
                longitude: 31.2357);

            await SeedProviderAsync(
                userManager, context,
                email: "seed.hospital@medlink.test",
                userName: "seed_hospital",
                serviceName: "Hospital",
                description: "General hospital for testing search",
                providerType: ProviderType.Hospital,
                latitude: 30.0500,
                longitude: 31.2400);

            await SeedProviderAsync(
                userManager, context,
                email: "seed.lab@medlink.test",
                userName: "seed_lab",
                serviceName: "Laboratory",
                description: "Medical testing laboratory for testing search",
                providerType: ProviderType.Lab,
                latitude: 30.0410,
                longitude: 31.2300);
        }

        private static async Task SeedProviderAsync(
            UserManager<AppUser> userManager,
            AppDbContext context,
            string email,
            string userName,
            string serviceName,
            string description,
            ProviderType providerType,
            double latitude,
            double longitude)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return;

            var user = new AppUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = "01000000000",
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(user, "Seed@12345");
            if (!createResult.Succeeded)
                return;

            await userManager.AddToRoleAsync(user, Roles.Provider);

            var providerProfile = new ProviderProfile
            {
                userId = user.Id,
                NationalIdNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
                ProviderType = providerType,
                TaxNumber = Guid.NewGuid().ToString("N").Substring(0, 10),
                LicenseFileId = Guid.NewGuid(),
                verificationStatus = VerificationStatus.Approved
            };

            context.ProviderProfiles.Add(providerProfile);

            var providerService = new ProviderService
            {
                ServiceName = serviceName,
                Description = description,
                IsActive = true,
                ProviderProfileId = providerProfile.Id
            };

            context.ProviderServices.Add(providerService);

            var providerLocation = new ProviderLocations
            {
                ProviderProfileId = providerProfile.Id,
                BranchName = $"{serviceName} - Seed Branch",
                Address = "Seed Test Address, Cairo",
                Latitude = latitude,
                Longitude = longitude,
                PostalCode = "11511",
                ServicePhoneNumber = "01000000000",
                IsActive = true
            };

            context.providerLocations.Add(providerLocation);

            await context.SaveChangesAsync();
        }
    }
}
