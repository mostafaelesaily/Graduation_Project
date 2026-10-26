using MedLink.Domain.Entities.Common;
using MedLink.Domain.Entities.Common.Identity;
using MedLink.Domain.Entities.Common.Notifications;
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
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }
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

            // Configure ProviderProfile entity Start :
            builder.Entity<ProviderProfile>(entity =>
            {
                entity.ToTable("ProviderProfiles"); 
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProviderName).IsRequired().HasMaxLength(255);
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

            // Configure Notification entity Start :
            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Source).IsRequired();
                entity.Property(e => e.Type).IsRequired().HasMaxLength(Notification.TypeMaxLength);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(Notification.TitleMaxLength);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(Notification.MessageMaxLength);
                entity.Property(e => e.Link).HasMaxLength(Notification.LinkMaxLength);
                entity.Property(e => e.TargetAudience);
                entity.Property(e => e.CreatedByUserId);
                entity.HasOne<AppUser>().WithMany().HasForeignKey(n => n.CreatedByUserId).OnDelete(DeleteBehavior.SetNull);
                entity.Property(e => e.CreatedAtUtc).IsRequired();
                entity.HasIndex(n => new { n.CreatedByUserId, n.CreatedAtUtc });
            });
            // Configure Notification entity End .

            // Configure NotificationRecipient entity Start :
            builder.Entity<NotificationRecipient>(entity =>
            {
                entity.ToTable("NotificationRecipients");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.IsHidden).HasDefaultValue(false);
                entity.Property(e => e.CreatedAtUtc).IsRequired();
                entity.HasOne(r => r.Notification)
                    .WithMany(n => n.Recipients)
                    .HasForeignKey(r => r.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(r => new { r.NotificationId, r.UserId }).IsUnique();
                entity.HasIndex(r => new { r.UserId, r.IsHidden, r.CreatedAtUtc });
            });
            // Configure NotificationRecipient entity End .

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
            // Relationship End .




        }
    }
    
}
