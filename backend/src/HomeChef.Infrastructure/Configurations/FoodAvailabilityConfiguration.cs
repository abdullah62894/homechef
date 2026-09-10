using HomeChef.Domain.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FoodAvailabilityConfiguration : IEntityTypeConfiguration<FoodAvailability>
{
    public void Configure(EntityTypeBuilder<FoodAvailability> builder)
    {
        builder.ToTable("FoodAvailabilities");

        builder.HasKey(fa => fa.Id);

        builder.HasOne(fa => fa.FoodItem)
            .WithMany()
            .HasForeignKey(fa => fa.FoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fa => fa.MealCategory)
            .WithMany()
            .HasForeignKey(fa => fa.MealCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(fa => fa.AvailableDays)
            .HasColumnType("integer[]")
            .IsRequired();

        builder.Property(fa => fa.StartTime)
            .IsRequired();

        builder.Property(fa => fa.EndTime)
            .IsRequired();

        builder.Property(fa => fa.IsAllDay)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(fa => fa.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(fa => fa.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(fa => fa.FoodItemId);
        builder.HasIndex(fa => fa.MealCategoryId);
    }
}
