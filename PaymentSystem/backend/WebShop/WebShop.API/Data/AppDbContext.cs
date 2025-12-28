using Microsoft.EntityFrameworkCore;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<InsurancePackage> InsurancePackages { get; set; }
        public DbSet<AdditionalService> AdditionalServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User entity configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired();
            });

            // InsurancePackage entity configuration
            modelBuilder.Entity<InsurancePackage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.PricePerDay).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.CoverageLimit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Deductible).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });

            // AdditionalService entity configuration
            modelBuilder.Entity<AdditionalService>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PricePerDay).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.IsAvailable).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.IconUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });
        }
    }
}
