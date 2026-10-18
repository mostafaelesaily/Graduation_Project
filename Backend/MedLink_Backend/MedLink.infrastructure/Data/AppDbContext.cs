using MedLink.Domain.Entities.Common.File;
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.HasDefaultSchema("Shared");

            builder.Entity<Notification>(entity =>
            {

                entity.HasKey(n => n.Id);

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

        }
    }
    
}
