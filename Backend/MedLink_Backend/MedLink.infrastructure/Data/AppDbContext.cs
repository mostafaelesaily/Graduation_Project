using MedLink.Domain.Entities.Common;
using MedLink.Domain.Enums;
using MedLink.Domain.Entities.Common.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Shared");
            // Configure storedFiles entity Start : 

            builder.Entity<StoredFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
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
                    .HasFilter("([Status] <> 2 AND [Status] <> 4)");
            });
            // Configure Booking entity End .

            // Configure ProviderProfile entity Start :
            builder.Entity<ProviderProfile>(entity =>
            {
                entity.ToTable("ProviderProfiles"); 
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.NationalIdNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TaxNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
            // Configure ProviderProfile entity End .   

            // Configure ProviderLocations entity Start : 
            builder.Entity<ProviderLocations>(entity =>
            {
                entity.ToTable("ProviderLocations");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Address).IsRequired().HasMaxLength(500);
                entity.Property(x => x.Longitude);   // nullable
                entity.Property(x => x.Latitude);    // nullable
                entity.Property(x => x.IsActive).HasDefaultValue(true);
            });
            // Configure ProviderLocations entity End .

            // Configure AppUser entity Start :
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AppUsers");
                entity.Property(x => x.IsActice).HasDefaultValue(false);
            });
            // Configure AppUser entity End .

            // Relationships Start : 
            // Configure one-to-one relationship between ProviderProfile and AppUser
            builder.Entity<ProviderProfile>()
                .HasOne(p => p.user)
                .WithOne(u => u.providerProfile)
                .HasForeignKey<ProviderProfile>(p => p.userId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between ProviderProfile and ProviderLocations
            builder.Entity<ProviderLocations>()
                .HasOne(pl => pl.ProviderProfile)
                .WithMany(p => p.ProviderLocations)
                .HasForeignKey(pl => pl.ProviderProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            // Configure one-to-many relationship between AppUser and Notifications
            builder.Entity<Notifications>()
                .HasOne(n => n.User)
                .WithMany(u => u.notifications)
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
            // Relationship End .




        }
    }
    
}
