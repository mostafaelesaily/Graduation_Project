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


        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ProviderProfile> ProviderProfiles { get; set; }
        public DbSet<ProviderLocations> providerLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Shared");


            builder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications", "Shared");

                entity.HasKey(n => n.Id);

                entity.Property(n => n.Id)
                    .HasColumnType("uniqueidentifier");

                entity.Property(n => n.Title)
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(n => n.Message)
                    .HasMaxLength(1000)
                    .IsRequired();

                entity.Property(n => n.Type)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(n => n.UserId)
                    .IsRequired();

                entity.Property(n => n.Link)
                    .HasMaxLength(500);

                entity.HasIndex(n => new
                {
                    n.UserId,
                    n.IsRead,
                    n.CreatedAtUtc
                });

                entity.HasOne<MedLink.Domain.Entities.Common.Identity.AppUser>()
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.Property(n => n.CreatedAtUtc)
    .HasConversion(new ValueConverter<DateTime, DateTime>(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)));

                entity.Property(n => n.ReadAtUtc)
                    .HasConversion(new ValueConverter<DateTime?, DateTime?>(
                        v => v,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v));
            });

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
                entity.Property(x => x.Longitude).IsRequired().HasMaxLength(255);
                entity.Property(x => x.Latitude).IsRequired().HasMaxLength(255);
            });
            // Configure ProviderLocations entity End .

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
