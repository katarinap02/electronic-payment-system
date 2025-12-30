using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.Property(e => e.Type).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.IsActive).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
