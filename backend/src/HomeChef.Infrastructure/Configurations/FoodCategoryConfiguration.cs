using HomeChef.Domain.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FoodCategoryConfiguration : IEntityTypeConfiguration<FoodCategory>
{
    public void Configure(EntityTypeBuilder<FoodCategory> builder)
    {
        builder.ToTable("FoodCategories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Name = "Main Course",
                Slug = "main-course",
                Description = "Hearty, home-style traditional and continental main meals.",
                DisplayOrder = 1,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Name = "Rice & Biryani",
                Slug = "rice-biryani",
                Description = "Aromatic spiced rice, authentic biryanis, and pulao specialties.",
                DisplayOrder = 2,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Name = "Karahi & Curries",
                Slug = "karahi-curries",
                Description = "Rich desi gravies, fresh wok karahis, and slow-cooked curries.",
                DisplayOrder = 3,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                Name = "BBQ & Grills",
                Slug = "bbq-grills",
                Description = "Charcoal grilled kebabs, tikkas, and smoked specialties.",
                DisplayOrder = 4,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                Name = "Bakery & Cakes",
                Slug = "bakery-cakes",
                Description = "Freshly baked customized cakes, artisanal breads, pastries, and cookies.",
                DisplayOrder = 5,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                Name = "Desserts & Sweets",
                Slug = "desserts-sweets",
                Description = "Decadent puddings, traditional mithai, brownies, and treats.",
                DisplayOrder = 6,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111107"),
                Name = "Snacks & Appetizers",
                Slug = "snacks-appetizers",
                Description = "Crispy samosas, rolls, chaat, and quick savory bites.",
                DisplayOrder = 7,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111108"),
                Name = "Breakfast & Parathas",
                Slug = "breakfast-parathas",
                Description = "Morning favorites, stuffed parathas, halwa puri, and omelettes.",
                DisplayOrder = 8,
                CreatedAtUtc = seedDate,
            },
            new FoodCategory
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111109"),
                Name = "Beverages",
                Slug = "beverages",
                Description = "Homemade drinks, lassi, fresh juices, and specialty teas.",
                DisplayOrder = 9,
                CreatedAtUtc = seedDate,
            }
        );
    }
}
