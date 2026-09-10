using HomeChef.Domain.Chefs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ChefAvailabilityConfiguration : IEntityTypeConfiguration<ChefAvailability>
{
    public void Configure(EntityTypeBuilder<ChefAvailability> builder)
    {
        builder.ToTable("ChefAvailabilities");

        builder.HasKey(a => a.Id);

        builder.HasOne(a => a.ChefProfile)
            .WithMany()
            .HasForeignKey(a => a.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(a => a.DayOfWeek)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(a => a.OpenTime)
            .IsRequired();

        builder.Property(a => a.CloseTime)
            .IsRequired();

        builder.Property(a => a.Label)
            .HasMaxLength(50);

        builder.Property(a => a.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(a => a.ChefProfileId);
        builder.HasIndex(a => new { a.ChefProfileId, a.DayOfWeek });
    }
}
