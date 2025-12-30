using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class WebShopAdminConfiguration : IEntityTypeConfiguration<WebShopAdmin>
{
    public void Configure(EntityTypeBuilder<WebShopAdmin> builder)
    {
        builder.HasKey(wa => new { wa.UserId, wa.WebShopId });

        builder.HasOne(wa => wa.User)
            .WithMany(u => u.ManagedWebShops)
            .HasForeignKey(wa => wa.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wa => wa.WebShop)
            .WithMany(w => w.Admins)
            .HasForeignKey(wa => wa.WebShopId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(wa => wa.AssignedAt)
            .IsRequired();
    }
}
