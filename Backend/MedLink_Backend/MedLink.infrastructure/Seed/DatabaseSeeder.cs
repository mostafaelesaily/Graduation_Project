using MedLink.Application.Constants;
using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Medical_History;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Enums;
using MedLink.infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedLink.infrastructure.Seed
{
    /// <summary>
    /// Seeds development/testing data into every table of the database
    /// (users, providers, bookings, reviews, medical records, notifications...).
    /// Every seeded account (other than Admin) holds BOTH the "User" and
    /// "Provider" roles and has data on both sides (patient bookings/reviews/
    /// records AND its own ProviderProfile/location/service/availability),
    /// so a single login/token can be used to act as patient or provider.
    /// Safe to run multiple times: users are matched by email, roles are
    /// added if missing, and the transactional data (bookings/reviews/
    /// records/notifications) is only inserted once per account.
    /// </summary>
    public static class DatabaseSeeder
    {
        private const string DefaultPassword = "Seed@12345";

        public static async Task SeedAsync(UserManager<AppUser> userManager, AppDbContext context)
        {
            var admin = await SeedUserAsync(userManager, "admin@medlink.com", "admin", "01100000000", new[] { Roles.Admin });

            // Each account below is simultaneously a patient (User) and a provider (Provider).
            var account1 = await SeedUserWithProviderAsync(
                userManager, context,
                email: "user1@medlink.com", userName: "user1", phoneNumber: "01300000001",
                serviceName: "Nile Clinic", serviceDescription: "General outpatient clinic",
                providerType: ProviderType.Clinic, latitude: 30.0444, longitude: 31.2357, address: "Downtown, Cairo");

            var account2 = await SeedUserWithProviderAsync(
                userManager, context,
                email: "user2@medlink.com", userName: "user2", phoneNumber: "01300000002",
                serviceName: "Al Salam Hospital", serviceDescription: "Multi-specialty hospital",
                providerType: ProviderType.Hospital, latitude: 30.0626, longitude: 31.2497, address: "Heliopolis, Cairo");

            var account3 = await SeedUserWithProviderAsync(
                userManager, context,
                email: "user3@medlink.com", userName: "user3", phoneNumber: "01300000003",
                serviceName: "MedLab Diagnostics", serviceDescription: "Medical testing laboratory",
                providerType: ProviderType.Lab, latitude: 30.0131, longitude: 31.2089, address: "Giza");

            await SeedTransactionalDataAsync(context, admin, account1, account2, account3);
        }

        private static async Task<AppUser> SeedUserAsync(
            UserManager<AppUser> userManager,
            string email,
            string userName,
            string phoneNumber,
            string[] roles)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new AppUser
                {
                    UserName = userName,
                    Email = email,
                    EmailConfirmed = true,
                    PhoneNumber = phoneNumber,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, DefaultPassword);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Failed to seed user '{email}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var existingRoles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                if (!existingRoles.Contains(role))
                    await userManager.AddToRoleAsync(user, role);
            }

            return user;
        }

        private static async Task<(AppUser User, ProviderProfile Profile)> SeedUserWithProviderAsync(
            UserManager<AppUser> userManager,
            AppDbContext context,
            string email,
            string userName,
            string phoneNumber,
            string serviceName,
            string serviceDescription,
            ProviderType providerType,
            double latitude,
            double longitude,
            string address)
        {
            var user = await SeedUserAsync(userManager, email, userName, phoneNumber, new[] { Roles.User, Roles.Provider });

            var existingProfile = await context.ProviderProfiles.FirstOrDefaultAsync(p => p.userId == user.Id);
            if (existingProfile != null)
                return (user, existingProfile);

            var profile = new ProviderProfile
            {
                userId = user.Id,
                NationalIdNumber = Guid.NewGuid().ToString("N").Substring(0, 14),
                ProviderType = providerType,
                TaxNumber = Guid.NewGuid().ToString("N").Substring(0, 10),
                verificationStatus = VerificationStatus.Approved
            };

            var licenseFile = new StoredFile
            {
                FileName = $"{userName}_license.pdf",
                FilePath = $"/seed/files/{userName}_license.pdf",
                FileType = "application/pdf",
                ProviderProfileId = profile.Id
            };
            profile.LicenseFileId = licenseFile.Id;

            context.ProviderProfiles.Add(profile);
            context.storedFiles.Add(licenseFile);

            context.providerLocations.Add(new ProviderLocations
            {
                ProviderProfileId = profile.Id,
                BranchName = $"{serviceName} - Main Branch",
                Address = address,
                Latitude = latitude,
                Longitude = longitude,
                PostalCode = "11511",
                ServicePhoneNumber = phoneNumber,
                IsActive = true
            });

            context.ProviderServices.Add(new ProviderService
            {
                ServiceName = serviceName,
                Description = serviceDescription,
                IsActive = true,
                ProviderProfileId = profile.Id
            });

            foreach (var day in new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday })
            {
                context.ProviderAvailabilities.Add(new ProviderAvailability
                {
                    ProviderProfileId = profile.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(17, 0, 0)
                });
            }

            await context.SaveChangesAsync();

            return (user, profile);
        }

        private static async Task SeedTransactionalDataAsync(
            AppDbContext context,
            AppUser admin,
            (AppUser User, ProviderProfile Profile) account1,
            (AppUser User, ProviderProfile Profile) account2,
            (AppUser User, ProviderProfile Profile) account3)
        {
            var seededUserIds = new[] { account1.User.Id, account2.User.Id, account3.User.Id };
            if (await context.Bookings.AnyAsync(b => seededUserIds.Contains(b.UserId)))
                return;

            var today = DateTime.UtcNow.Date;

            // Each account books another account's provider profile (acting as a patient),
            // while also being bookable itself through its own ProviderProfile.
            context.Bookings.AddRange(
                new Booking { UserId = account1.User.Id, ProviderProfileId = account2.Profile.Id, BookingDate = today.AddDays(1).AddHours(10), Status = BookingStatus.Confirmed, Notes = "Routine checkup" },
                new Booking { UserId = account2.User.Id, ProviderProfileId = account3.Profile.Id, BookingDate = today.AddDays(2).AddHours(11), Status = BookingStatus.Pending, Notes = "First visit" },
                new Booking { UserId = account3.User.Id, ProviderProfileId = account1.Profile.Id, BookingDate = today.AddDays(-2).AddHours(9), Status = BookingStatus.Completed, Notes = "Follow-up consultation" },
                new Booking { UserId = account1.User.Id, ProviderProfileId = account3.Profile.Id, BookingDate = today.AddDays(3).AddHours(14), Status = BookingStatus.Pending, Notes = "Blood test" },
                new Booking { UserId = account2.User.Id, ProviderProfileId = account1.Profile.Id, BookingDate = today.AddDays(5).AddHours(9).AddMinutes(30), Status = BookingStatus.Cancelled, Notes = "Cancelled by patient" },
                new Booking { UserId = account3.User.Id, ProviderProfileId = account2.Profile.Id, BookingDate = today.AddDays(-5).AddHours(10), Status = BookingStatus.Rejected, Notes = "Provider unavailable" }
            );

            context.Reviews.AddRange(
                new Review { PatientId = account1.User.Id, ProviderId = account2.User.Id, Rating = 4.5m, Comment = "Great service and friendly staff." },
                new Review { PatientId = account2.User.Id, ProviderId = account3.User.Id, Rating = 5m, Comment = "Excellent hospital, highly recommend." },
                new Review { PatientId = account3.User.Id, ProviderId = account1.User.Id, Rating = 3.5m, Comment = "Good lab, a bit slow with results." },
                new Review { PatientId = account1.User.Id, ProviderId = account3.User.Id, Rating = 4m, Comment = "Clean facility and professional doctors." }
            );

            context.medicalRecords.AddRange(
                new MedicalRecord { PatientId = account1.User.Id, Title = "Annual Checkup", Description = "General annual health checkup.", Diagnosis = "Healthy, no issues found.", Notes = "No concerns, follow up next year.", RecordDate = today.AddMonths(-1) },
                new MedicalRecord { PatientId = account2.User.Id, Title = "Flu Treatment", Description = "Treatment for seasonal flu symptoms.", Diagnosis = "Seasonal Flu", Notes = "Prescribed rest and medication.", RecordDate = today.AddDays(-10) },
                new MedicalRecord { PatientId = account3.User.Id, Title = "Blood Test Results", Description = "Routine blood panel analysis.", Diagnosis = "Results within normal ranges.", Notes = "No further action required.", RecordDate = today.AddDays(-20) }
            );

            context.notifications.AddRange(
                new Notifications { UserId = admin.Id, Title = "Welcome to MedLink", Message = "Your admin account has been created." },
                new Notifications { UserId = account1.User.Id, Title = "Booking Confirmed", Message = "Your booking with Al Salam Hospital has been confirmed." },
                new Notifications { UserId = account2.User.Id, Title = "Booking Received", Message = "Your booking request has been received and is pending approval." },
                new Notifications { UserId = account3.User.Id, Title = "Booking Rejected", Message = "Your booking request was rejected by the provider." },
                new Notifications { UserId = account1.User.Id, Title = "New Booking Request", Message = "You have a new pending booking request on your clinic." },
                new Notifications { UserId = account2.User.Id, Title = "Welcome to MedLink", Message = "Your provider account has been approved." }
            );

            await context.SaveChangesAsync();
        }
    }
}
