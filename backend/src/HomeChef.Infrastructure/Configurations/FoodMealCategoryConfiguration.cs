using HomeChef.Domain.Meals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FoodMealCategoryConfiguration : IEntityTypeConfiguration<FoodMealCategory>
{
    public void Configure(EntityTypeBuilder<FoodMealCategory> builder)
    {
        builder.ToTable("FoodMealCategories");

        builder.HasKey(fmc => new { fmc.FoodItemId, fmc.MealCategoryId });

        builder.HasOne(fmc => fmc.FoodItem)
            .WithMany()
            .HasForeignKey(fmc => fmc.FoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fmc => fmc.MealCategory)
            .WithMany()
            .HasForeignKey(fmc => fmc.MealCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fmc => fmc.FoodItemId);
        builder.HasIndex(fmc => fmc.MealCategoryId);
    }
}
