using Microsoft.EntityFrameworkCore;
using BadgeManagement.Models;

namespace BadgeManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Configure Badge entity
            modelBuilder.Entity<Badge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Issuer).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CredentialJson).IsRequired();
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Badges)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Result entity
            modelBuilder.Entity<Result>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Results)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.Badge)
                      .WithMany()
                      .HasForeignKey(e => e.BadgeId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
