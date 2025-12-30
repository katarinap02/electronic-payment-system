using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PSP.Domain.Entities;

namespace PSP.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.PasswordHash).IsRequired();
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Surname).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Role).IsRequired();
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
