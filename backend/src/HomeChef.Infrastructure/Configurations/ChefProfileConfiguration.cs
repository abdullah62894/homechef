using HomeChef.Domain.Chefs;
using HomeChef.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ChefProfileConfiguration : IEntityTypeConfiguration<ChefProfile>
{
    public void Configure(EntityTypeBuilder<ChefProfile> builder)
    {
        builder.ToTable("ChefProfiles");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<ChefProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Bio)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(p => p.City)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Area)
            .HasMaxLength(100);

        builder.Property(p => p.Address)
            .HasMaxLength(250);

        builder.Property(p => p.Latitude);

        builder.Property(p => p.Longitude);

        builder.Property(p => p.Cuisines)
            .HasColumnType("text[]")
            .IsRequired();

        builder.Property(p => p.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(p => p.PhotoThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(p => p.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(p => p.City);
        builder.HasIndex(p => p.Area);
        builder.HasIndex(p => new { p.City, p.Area });
        builder.HasIndex(p => new { p.Latitude, p.Longitude });
    }
}