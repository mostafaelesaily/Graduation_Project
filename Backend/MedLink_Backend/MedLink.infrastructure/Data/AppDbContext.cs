using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using MedLink.Domain.Entities.Common.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MedLink.Domain.Entities.Provider;
using MedLink.Domain.Entities.Medical_History;


namespace MedLink.infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser> 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {   
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<StoredFile> storedFiles { get; set; }
        public DbSet<ProviderProfile> ProviderProfiles { get; set; }
        public DbSet<ProviderLocations> providerLocations { get; set; } 
        public DbSet<Notifications> notifications { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ProviderService> ProviderServices { get; set; }
        public DbSet<MedicalRecord> medicalRecords { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProviderAvailability> ProviderAvailabilities { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Shared");
            // Configure storedFiles entity Start :

            builder.Entity<StoredFile>(entity =>
            {
                entity.ToTable("StoredFiles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ProviderProfileId).IsRequired();
                entity.HasIndex(e => e.ProviderProfileId);
            });
            // Configure storedFiles entity End .

            // Configure Notification Start :
            builder.Entity<Notifications>().ToTable("Notification");
            builder.Entity<Notifications>().HasKey(n => n.Id);
            builder.Entity<Notifications>().HasIndex(n => n.UserId);
            builder.Entity<Notifications>().HasIndex(n => n.Title);
            builder.Entity<Notifications>().HasIndex(n => new { n.UserId, n.IsRead });
            builder.Entity<Notifications>().Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Entity<Notifications>().Property(n => n.Message).HasMaxLength(2000);
            // Configure Notification End.

            // Configure Booking entity Start :
            builder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.RowVersion).IsRowVersion();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.ProviderProfileId, e.BookingDate })
                    .IsUnique()
                    .HasFilter("([Status] <> 2 AND [Status] <> 4 AND [Status] <> 5)");
            });
            // Configure Booking entity End .

            // Configure ProviderAvailability entity Start :
            builder.Entity<ProviderAvailability>(entity =>
            {
                entity.ToTable("ProviderAvailabilities");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DayOfWeek).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.HasIndex(e => new { e.ProviderProfileId, e.DayOfWeek, e.StartTime, e.EndTime }).IsUnique();
            });
            // Configure ProviderAvailability entity End .
            // Configure ProviderProfile entity Start :
            builder.Entity<ProviderProfile>(entity =>
            {
                entity.ToTable("ProviderProfiles");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NationalIdNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TaxNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.userId);
                entity.HasIndex(e => e.verificationStatus);
            });
            // Configure ProviderProfile entity End .   

            // Configure ProviderService entity Start :
            builder.Entity<ProviderService>(entity =>
            {
                entity.ToTable("ProviderServices");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.ProviderProfileId);
            });
            // Configure ProviderService entity End .

            // Configure ProviderLocations entity Start :
            builder.Entity<ProviderLocations>(entity =>
            {
                entity.ToTable("ProviderLocations");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Address).IsRequired().HasMaxLength(500);
                entity.Property(x => x.Longitude);   // nullable
                entity.Property(x => x.Latitude);    // nullable
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasIndex(x => x.ProviderProfileId);
            });
            // Configure ProviderLocations entity End .

            // Configure MedicalRecord entity Start :
            builder.Entity<MedicalRecord>(entity =>
            {
                entity.ToTable("MedicalRecords");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Diagnosis).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Notes).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.RecordDate).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.PatientId);
            });
            // Configure MedicalRecord entity End .

            // Configure AppUser entity Start :
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.Property(x => x.IsActive).HasDefaultValue(false);
                entity.HasIndex(x => x.Email).IsUnique();
            });
            // Configure AppUser entity End .

            // Configure Review entity Start :
            builder.Entity<Review>(entity =>
            {
                entity.ToTable("Reviews");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rating).HasPrecision(3, 2).IsRequired();
                entity.Property(e => e.Comment).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasIndex(e => e.PatientId);
                entity.HasIndex(e => e.ProviderId);
                entity.HasIndex(e => new { e.ProviderId, e.CreatedAt });
            });
            // Configure Review entity End .

            // Configure AppUser entity End .

            // Relationships Start : 
            // Configure one-to-one relationship between ProviderProfile and AppUser
            builder.Entity<ProviderProfile>()
                .HasOne(p => p.user)
                .WithOne(u => u.providerProfile)
                .HasForeignKey<ProviderProfile>(p => p.userId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between ProviderProfile and ProviderService
            builder.Entity<ProviderService>()
                .HasOne(s => s.ProviderProfile)
                .WithMany(p => p.ProviderServices)
                .HasForeignKey(s => s.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between ProviderProfile and ProviderLocations
            builder.Entity<ProviderLocations>()
                .HasOne(pl => pl.ProviderProfile)
                .WithMany(p => p.ProviderLocations)
                .HasForeignKey(pl => pl.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between ProviderProfile and StoredFile
            builder.Entity<StoredFile>()
                .HasOne(sf => sf.ProviderProfile)
                .WithMany(p => p.StoredFiles)
                .HasForeignKey(sf => sf.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between AppUser and MedicalRecord
            builder.Entity<MedicalRecord>()
                .HasOne(record => record.Patient)
                .WithMany(user => user.MedicalRecords)
                .HasForeignKey(record => record.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between AppUser and PatientReviews
            builder.Entity<Review>()
                .HasOne(review => review.Patient)
                .WithMany(user => user.PatientReviews)
                .HasForeignKey(review => review.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
            // Configure one-to-many relationship between AppUser and ProviderReviews
            builder.Entity<Review>()
                .HasOne(review => review.Provider)
                .WithMany(user => user.ProviderReviews)
                .HasForeignKey(review => review.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
            // Configure one-to-many relationship between AppUser and Notifications
            builder.Entity<Notifications>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship between AppUser and Bookings
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship between ProviderProfile and Bookings
            builder.Entity<Booking>()
                .HasOne(b => b.ProviderProfile)
                .WithMany(p => p.Bookings)
                .HasForeignKey(b => b.ProviderProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure one-to-many relationship between ProviderProfile and ProviderAvailability
            builder.Entity<ProviderAvailability>()
                .HasOne(a => a.ProviderProfile)
                .WithMany(p => p.ProviderAvailabilities)
                .HasForeignKey(a => a.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Relationship End .




        }
    }
    
}
