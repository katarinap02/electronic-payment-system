using Microsoft.EntityFrameworkCore;
using PSP.API.Models;

namespace PSP.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<WebShop> WebShops { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<WebShopPaymentMethod> WebShopPaymentMethods { get; set; }

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
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // WebShop entity configuration
            modelBuilder.Entity<WebShop>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ApiKey).IsRequired().HasMaxLength(500);
                entity.Property(e => e.MerchantId).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.MerchantId).IsUnique();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // PaymentMethod entity configuration
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // WebShopPaymentMethod entity configuration (Many-to-Many)
            modelBuilder.Entity<WebShopPaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => new { e.WebShopId, e.PaymentMethodId }).IsUnique();
                
                entity.HasOne(e => e.WebShop)
                      .WithMany(w => w.WebShopPaymentMethods)
                      .HasForeignKey(e => e.WebShopId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.PaymentMethod)
                      .WithMany(p => p.WebShopPaymentMethods)
                      .HasForeignKey(e => e.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(e => e.IsEnabled).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}
