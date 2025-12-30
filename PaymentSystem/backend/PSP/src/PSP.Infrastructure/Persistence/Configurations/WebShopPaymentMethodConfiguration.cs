using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class WebShopPaymentMethodConfiguration : IEntityTypeConfiguration<WebShopPaymentMethod>
{
    public void Configure(EntityTypeBuilder<WebShopPaymentMethod> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.HasIndex(e => new { e.WebShopId, e.PaymentMethodId }).IsUnique();
        
        builder.HasOne(e => e.WebShop)
              .WithMany(w => w.WebShopPaymentMethods)
              .HasForeignKey(e => e.WebShopId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.PaymentMethod)
              .WithMany(p => p.WebShopPaymentMethods)
              .HasForeignKey(e => e.PaymentMethodId)
              .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(e => e.IsEnabled).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
