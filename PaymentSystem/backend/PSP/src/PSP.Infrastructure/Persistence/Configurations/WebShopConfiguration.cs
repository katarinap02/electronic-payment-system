using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class WebShopConfiguration : IEntityTypeConfiguration<WebShop>
{
    public void Configure(EntityTypeBuilder<WebShop> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Url).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ApiKey).IsRequired().HasMaxLength(500);
        builder.Property(e => e.MerchantId).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.MerchantId).IsUnique();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
