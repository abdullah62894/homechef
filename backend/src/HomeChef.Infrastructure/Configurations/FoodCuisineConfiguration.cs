using HomeChef.Domain.Cuisines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FoodCuisineConfiguration : IEntityTypeConfiguration<FoodCuisine>
{
    public void Configure(EntityTypeBuilder<FoodCuisine> builder)
    {
        builder.ToTable("FoodCuisines");

        builder.HasKey(fc => new { fc.FoodItemId, fc.CuisineId });

        builder.HasOne(fc => fc.FoodItem)
            .WithMany()
            .HasForeignKey(fc => fc.FoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fc => fc.Cuisine)
            .WithMany()
            .HasForeignKey(fc => fc.CuisineId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fc => fc.FoodItemId);
        builder.HasIndex(fc => fc.CuisineId);
    }
}
