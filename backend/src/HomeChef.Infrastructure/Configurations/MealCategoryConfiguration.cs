using HomeChef.Domain.Meals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class MealCategoryConfiguration : IEntityTypeConfiguration<MealCategory>
{
    public void Configure(EntityTypeBuilder<MealCategory> builder)
    {
        builder.ToTable("MealCategories");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Slug)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(m => m.Slug)
            .IsUnique();

        builder.Property(m => m.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new MealCategory
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333301"),
                Name = "Breakfast",
                Slug = "breakfast",
                DisplayOrder = 1,
                CreatedAtUtc = seedDate,
            },
            new MealCategory
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333302"),
                Name = "Lunch",
                Slug = "lunch",
                DisplayOrder = 2,
                CreatedAtUtc = seedDate,
            },
            new MealCategory
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333303"),
                Name = "Dinner",
                Slug = "dinner",
                DisplayOrder = 3,
                CreatedAtUtc = seedDate,
            }
        );
    }
}
