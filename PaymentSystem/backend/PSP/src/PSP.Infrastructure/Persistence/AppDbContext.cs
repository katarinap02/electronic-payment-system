using Microsoft.EntityFrameworkCore;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<WebShop> WebShops { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<WebShopPaymentMethod> WebShopPaymentMethods { get; set; }
    public DbSet<WebShopAdmin> WebShopAdmins { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
