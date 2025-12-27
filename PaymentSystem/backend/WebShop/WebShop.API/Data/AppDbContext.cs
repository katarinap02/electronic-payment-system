using Microsoft.EntityFrameworkCore;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

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

            // Vehicle entity configuration
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.PricePerDay).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Transmission).IsRequired();
                entity.Property(e => e.FuelType).IsRequired();
                entity.Property(e => e.Seats).IsRequired();
                entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.LicensePlate).IsUnique();
                entity.Property(e => e.Status).IsRequired().HasDefaultValue(VehicleStatus.Available);
                entity.Property(e => e.Mileage).IsRequired();
                entity.Property(e => e.Color).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
            });
        }
    }
}
