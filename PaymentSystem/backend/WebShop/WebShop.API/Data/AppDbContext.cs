using Microsoft.EntityFrameworkCore;
using WebShop.API.Models;

namespace WebShop.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<InsurancePackage> InsurancePackages { get; set; }
        public DbSet<AdditionalService> AdditionalServices { get; set; }
        public DbSet<Rental> Rentals { get; set; }

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

            // Rental entity configuration
            modelBuilder.Entity<Rental>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Foreign keys
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Vehicle)
                    .WithMany()
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Properties
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.RentalDays).IsRequired();
                entity.Property(e => e.AdditionalServices).HasMaxLength(1000);
                entity.Property(e => e.AdditionalServicesPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InsuranceType).HasMaxLength(50);
                entity.Property(e => e.InsurancePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VehiclePricePerDay).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GlobalTransactionId).HasMaxLength(100);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                
                // Indexes
                entity.HasIndex(e => e.PaymentId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.Status);
            });
        }
    }
}
