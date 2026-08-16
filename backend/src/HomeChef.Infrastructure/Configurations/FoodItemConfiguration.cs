using HomeChef.Domain.Chefs;
using HomeChef.Domain.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("FoodItems");

        builder.HasKey(f => f.Id);

        builder.HasOne(f => f.ChefProfile)
            .WithMany()
            .HasForeignKey(f => f.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Category)
            .WithMany()
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(f => f.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(f => f.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(f => f.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("PKR")
            .IsRequired();

        builder.Property(f => f.IsAvailable)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(f => f.ImageUrl)
            .HasMaxLength(500);

        builder.Property(f => f.PreparationTimeMinutes);

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(f => f.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(f => f.ChefProfileId);
        builder.HasIndex(f => f.CategoryId);
        builder.HasIndex(f => f.IsAvailable);
        builder.HasIndex(f => f.CreatedAtUtc);
    }
}
