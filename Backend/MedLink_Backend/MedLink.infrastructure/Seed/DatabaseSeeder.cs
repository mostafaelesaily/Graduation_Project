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
    /// This also covers what used to be MedicalSearchTestSeeder (the Clinic/
    /// Hospital/Lab search-test providers), now merged in as regular dual
    /// accounts so there is a single seed source for every table.
    /// Safe to run multiple times: users are matched by email, roles are
    /// added if missing, and the transactional data (bookings/reviews/
    /// records/notifications) is only inserted once per account.
    /// </summary>
    public static class DatabaseSeeder
    {
        private const string DefaultPassword = "Seed@12345";

        private record ProviderAccountSeed(
            string Email, string UserName, string PhoneNumber,
            string ServiceName, string ServiceDescription, ProviderType ProviderType,
            double Latitude, double Longitude, string Address);

        private static readonly ProviderAccountSeed[] ProviderAccounts =
        {
            new("user1@medlink.com", "user1", "01300000001", "Nile Clinic", "General outpatient clinic", ProviderType.Clinic, 30.0444, 31.2357, "Downtown, Cairo"),
            new("user2@medlink.com", "user2", "01300000002", "Al Salam Hospital", "Multi-specialty hospital", ProviderType.Hospital, 30.0626, 31.2497, "Heliopolis, Cairo"),
            new("user3@medlink.com", "user3", "01300000003", "MedLab Diagnostics", "Medical testing laboratory", ProviderType.Lab, 30.0131, 31.2089, "Giza"),
            new("user4@medlink.com", "user4", "01300000004", "Zahra Clinic", "General medical clinic", ProviderType.Clinic, 30.0500, 31.2400, "Nasr City, Cairo"),
            new("user5@medlink.com", "user5", "01300000005", "Dar El Shifa Hospital", "General hospital", ProviderType.Hospital, 30.0410, 31.2300, "Mohandessin, Giza"),
            new("user6@medlink.com", "user6", "01300000006", "BioCheck Laboratory", "Medical testing laboratory", ProviderType.Lab, 30.0300, 31.2500, "Maadi, Cairo"),
            new("user7@medlink.com", "user7", "01300000007", "Sunrise Family Clinic", "Family medicine clinic", ProviderType.Clinic, 30.0600, 31.2200, "6th of October City, Giza"),
            new("user8@medlink.com", "user8", "01300000008", "Cairo Specialized Hospital", "Multi-specialty hospital", ProviderType.Hospital, 30.0450, 31.2600, "Mokattam, Cairo"),
            new("user9@medlink.com", "user9", "01300000009", "PrecisionDx Lab", "Medical testing laboratory", ProviderType.Lab, 31.2001, 29.9187, "Alexandria"),
            new("user10@medlink.com", "user10", "01300000010", "New Horizon Clinic", "General outpatient clinic", ProviderType.Clinic, 30.0620, 31.2200, "Zamalek, Cairo"),
        };

        public static async Task SeedAsync(UserManager<AppUser> userManager, AppDbContext context)
        {
            var admin = await SeedUserAsync(userManager, "admin@medlink.com", "admin", "01100000000", new[] { Roles.Admin });

            // Each account below is simultaneously a patient (User) and a provider (Provider).
            var accounts = new List<(AppUser User, ProviderProfile Profile)>();
            foreach (var seed in ProviderAccounts)
            {
                var account = await SeedUserWithProviderAsync(
                    userManager, context,
                    email: seed.Email, userName: seed.UserName, phoneNumber: seed.PhoneNumber,
                    serviceName: seed.ServiceName, serviceDescription: seed.ServiceDescription,
                    providerType: seed.ProviderType, latitude: seed.Latitude, longitude: seed.Longitude, address: seed.Address);
                accounts.Add(account);
            }

            await SeedTransactionalDataAsync(context, admin, accounts);
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
            List<(AppUser User, ProviderProfile Profile)> accounts)
        {
            var seededUserIds = accounts.Select(a => a.User.Id).ToArray();
            if (await context.Bookings.AnyAsync(b => seededUserIds.Contains(b.UserId)))
                return;

            var today = DateTime.UtcNow.Date;
            var u = accounts; // short alias: u[i].User is the patient, u[i].Profile is their own clinic/hospital/lab

            // Each account books another account's provider profile (acting as a patient),
            // while also being bookable itself through its own ProviderProfile.
            context.Bookings.AddRange(
                new Booking { UserId = u[0].User.Id, ProviderProfileId = u[1].Profile.Id, BookingDate = today.AddDays(1).AddHours(10), Status = BookingStatus.Confirmed, Notes = "Routine checkup" },
                new Booking { UserId = u[1].User.Id, ProviderProfileId = u[2].Profile.Id, BookingDate = today.AddDays(2).AddHours(11), Status = BookingStatus.Pending, Notes = "First visit" },
                new Booking { UserId = u[2].User.Id, ProviderProfileId = u[0].Profile.Id, BookingDate = today.AddDays(-2).AddHours(9), Status = BookingStatus.Completed, Notes = "Follow-up consultation" },
                new Booking { UserId = u[0].User.Id, ProviderProfileId = u[2].Profile.Id, BookingDate = today.AddDays(3).AddHours(14), Status = BookingStatus.Pending, Notes = "Blood test" },
                new Booking { UserId = u[1].User.Id, ProviderProfileId = u[0].Profile.Id, BookingDate = today.AddDays(5).AddHours(9).AddMinutes(30), Status = BookingStatus.Cancelled, Notes = "Cancelled by patient" },
                new Booking { UserId = u[2].User.Id, ProviderProfileId = u[1].Profile.Id, BookingDate = today.AddDays(-5).AddHours(10), Status = BookingStatus.Rejected, Notes = "Provider unavailable" },
                new Booking { UserId = u[3].User.Id, ProviderProfileId = u[4].Profile.Id, BookingDate = today.AddDays(1).AddHours(11), Status = BookingStatus.Confirmed, Notes = "General consultation" },
                new Booking { UserId = u[4].User.Id, ProviderProfileId = u[5].Profile.Id, BookingDate = today.AddDays(4).AddHours(10).AddMinutes(30), Status = BookingStatus.Pending, Notes = "Lab referral" },
                new Booking { UserId = u[5].User.Id, ProviderProfileId = u[3].Profile.Id, BookingDate = today.AddDays(-3).AddHours(15), Status = BookingStatus.Completed, Notes = "Skin checkup" },
                new Booking { UserId = u[6].User.Id, ProviderProfileId = u[7].Profile.Id, BookingDate = today.AddDays(2).AddHours(9), Status = BookingStatus.Confirmed, Notes = "Cardiology consultation" },
                new Booking { UserId = u[7].User.Id, ProviderProfileId = u[8].Profile.Id, BookingDate = today.AddDays(6).AddHours(13), Status = BookingStatus.Pending, Notes = "Routine blood panel" },
                new Booking { UserId = u[8].User.Id, ProviderProfileId = u[6].Profile.Id, BookingDate = today.AddDays(-1).AddHours(16), Status = BookingStatus.Completed, Notes = "Dermatology follow-up" },
                new Booking { UserId = u[9].User.Id, ProviderProfileId = u[0].Profile.Id, BookingDate = today.AddDays(7).AddHours(10), Status = BookingStatus.Pending, Notes = "New patient visit" },
                new Booking { UserId = u[3].User.Id, ProviderProfileId = u[9].Profile.Id, BookingDate = today.AddDays(-4).AddHours(9), Status = BookingStatus.Cancelled, Notes = "Rescheduled by patient" }
            );

            context.Reviews.AddRange(
                new Review { PatientId = u[0].User.Id, ProviderId = u[1].User.Id, Rating = 4.5m, Comment = "Great service and friendly staff." },
                new Review { PatientId = u[1].User.Id, ProviderId = u[2].User.Id, Rating = 5m, Comment = "Excellent lab, highly recommend." },
                new Review { PatientId = u[2].User.Id, ProviderId = u[0].User.Id, Rating = 3.5m, Comment = "Good clinic, a bit slow with results." },
                new Review { PatientId = u[0].User.Id, ProviderId = u[2].User.Id, Rating = 4m, Comment = "Clean facility and professional staff." },
                new Review { PatientId = u[3].User.Id, ProviderId = u[4].User.Id, Rating = 4.5m, Comment = "Caring doctors and short wait time." },
                new Review { PatientId = u[4].User.Id, ProviderId = u[5].User.Id, Rating = 5m, Comment = "Accurate and fast lab results." },
                new Review { PatientId = u[5].User.Id, ProviderId = u[3].User.Id, Rating = 4m, Comment = "Friendly clinic staff." },
                new Review { PatientId = u[6].User.Id, ProviderId = u[7].User.Id, Rating = 4.5m, Comment = "Professional hospital staff." },
                new Review { PatientId = u[7].User.Id, ProviderId = u[8].User.Id, Rating = 3.5m, Comment = "Lab results took longer than expected." },
                new Review { PatientId = u[8].User.Id, ProviderId = u[6].User.Id, Rating = 5m, Comment = "Excellent dermatology care." },
                new Review { PatientId = u[9].User.Id, ProviderId = u[0].User.Id, Rating = 4m, Comment = "Welcoming clinic for new patients." },
                new Review { PatientId = u[3].User.Id, ProviderId = u[9].User.Id, Rating = 4.5m, Comment = "Would recommend to family." }
            );

            context.medicalRecords.AddRange(
                new MedicalRecord { PatientId = u[0].User.Id, Title = "Annual Checkup", Description = "General annual health checkup.", Diagnosis = "Healthy, no issues found.", Notes = "No concerns, follow up next year.", RecordDate = today.AddMonths(-1) },
                new MedicalRecord { PatientId = u[1].User.Id, Title = "Flu Treatment", Description = "Treatment for seasonal flu symptoms.", Diagnosis = "Seasonal Flu", Notes = "Prescribed rest and medication.", RecordDate = today.AddDays(-10) },
                new MedicalRecord { PatientId = u[2].User.Id, Title = "Blood Test Results", Description = "Routine blood panel analysis.", Diagnosis = "Results within normal ranges.", Notes = "No further action required.", RecordDate = today.AddDays(-20) },
                new MedicalRecord { PatientId = u[3].User.Id, Title = "Allergy Screening", Description = "Screening for seasonal allergies.", Diagnosis = "Mild pollen allergy detected.", Notes = "Prescribed antihistamines.", RecordDate = today.AddDays(-15) },
                new MedicalRecord { PatientId = u[4].User.Id, Title = "Cardiac Evaluation", Description = "Routine cardiac evaluation.", Diagnosis = "Normal ECG readings.", Notes = "No further action required.", RecordDate = today.AddDays(-8) },
                new MedicalRecord { PatientId = u[5].User.Id, Title = "Skin Biopsy", Description = "Biopsy of a suspicious skin lesion.", Diagnosis = "Benign mole.", Notes = "No treatment needed.", RecordDate = today.AddDays(-12) },
                new MedicalRecord { PatientId = u[6].User.Id, Title = "Physical Therapy Assessment", Description = "Assessment for lower back pain.", Diagnosis = "Mild lumbar strain.", Notes = "Recommended physical therapy.", RecordDate = today.AddDays(-6) },
                new MedicalRecord { PatientId = u[7].User.Id, Title = "Vaccination Record", Description = "Seasonal flu vaccination.", Diagnosis = "Vaccine administered successfully.", Notes = "No adverse reaction observed.", RecordDate = today.AddDays(-3) },
                new MedicalRecord { PatientId = u[8].User.Id, Title = "Diabetes Screening", Description = "Routine diabetes screening.", Diagnosis = "Blood sugar within normal range.", Notes = "Recheck in 6 months.", RecordDate = today.AddDays(-25) },
                new MedicalRecord { PatientId = u[9].User.Id, Title = "Eye Examination", Description = "Routine vision checkup.", Diagnosis = "Mild myopia detected.", Notes = "Prescribed corrective lenses.", RecordDate = today.AddDays(-18) },
                new MedicalRecord { PatientId = u[0].User.Id, Title = "Dental Checkup", Description = "Routine dental examination.", Diagnosis = "No cavities found.", Notes = "Recommended checkup in 6 months.", RecordDate = today.AddDays(-30) },
                new MedicalRecord { PatientId = u[2].User.Id, Title = "Nutrition Consultation", Description = "Dietary assessment and consultation.", Diagnosis = "Mild vitamin D deficiency.", Notes = "Prescribed supplements.", RecordDate = today.AddDays(-22) }
            );

            context.notifications.AddRange(
                new Notifications { UserId = admin.Id, Title = "Welcome to MedLink", Message = "Your admin account has been created." },
                new Notifications { UserId = u[0].User.Id, Title = "Booking Confirmed", Message = "Your booking with Al Salam Hospital has been confirmed." },
                new Notifications { UserId = u[1].User.Id, Title = "Booking Received", Message = "Your booking request has been received and is pending approval." },
                new Notifications { UserId = u[2].User.Id, Title = "Booking Rejected", Message = "Your booking request was rejected by the provider." },
                new Notifications { UserId = u[0].User.Id, Title = "New Booking Request", Message = "You have a new pending booking request on your clinic." },
                new Notifications { UserId = u[1].User.Id, Title = "Welcome to MedLink", Message = "Your provider account has been approved." },
                new Notifications { UserId = u[3].User.Id, Title = "Booking Confirmed", Message = "Your general consultation booking has been confirmed." },
                new Notifications { UserId = u[4].User.Id, Title = "New Booking Request", Message = "You have a new pending booking request on your hospital." },
                new Notifications { UserId = u[6].User.Id, Title = "Booking Confirmed", Message = "Your cardiology consultation has been confirmed." },
                new Notifications { UserId = u[7].User.Id, Title = "New Booking Request", Message = "You have a new pending booking request on your hospital." },
                new Notifications { UserId = u[9].User.Id, Title = "Booking Pending", Message = "Your booking request is awaiting provider approval." },
                new Notifications { UserId = u[8].User.Id, Title = "Review Received", Message = "A patient left a new review on your lab." }
            );

            await context.SaveChangesAsync();
        }
    }
}
