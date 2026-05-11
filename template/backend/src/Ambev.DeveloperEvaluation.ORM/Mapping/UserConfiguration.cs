using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Password).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.Phone).HasMaxLength(20);

        builder.Property(u => u.FirstName).HasMaxLength(50);
        builder.Property(u => u.LastName).HasMaxLength(50);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.Street).HasMaxLength(200);
        builder.Property(u => u.Number);
        builder.Property(u => u.ZipCode).HasMaxLength(20);
        builder.Property(u => u.GeoLat).HasMaxLength(50);
        builder.Property(u => u.GeoLong).HasMaxLength(50);

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

    }
}
