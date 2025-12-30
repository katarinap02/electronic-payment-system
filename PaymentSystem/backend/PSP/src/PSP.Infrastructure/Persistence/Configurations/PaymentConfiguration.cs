using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.MerchantOrderId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.PaymentUrl)
            .HasMaxLength(500);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(p => p.WebShop)
            .WithMany()
            .HasForeignKey(p => p.WebShopId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PaymentMethod)
            .WithMany()
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.MerchantOrderId);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => new { p.WebShopId, p.MerchantOrderId });
    }
}
