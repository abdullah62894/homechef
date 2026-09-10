using HomeChef.Domain.Cuisines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class CuisineConfiguration : IEntityTypeConfiguration<Cuisine>
{
    public void Configure(EntityTypeBuilder<Cuisine> builder)
    {
        builder.ToTable("Cuisines");

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

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.ImageThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(c => c.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(c => c.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.DisplayOrder);

        var seedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        builder.HasData(
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222201"),
                Name = "Pakistani / Desi",
                Slug = "pakistani-desi",
                Description = "Authentic Pakistani and traditional desi home-cooked meals.",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222202"),
                Name = "Biryani",
                Slug = "biryani",
                Description = "Aromatic layered rice dishes with premium spices and tender meat.",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222203"),
                Name = "Chinese",
                Slug = "chinese",
                Description = "Indo-Chinese and authentic Chinese stir-fry, noodles, and rice.",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222204"),
                Name = "Fast Food",
                Slug = "fast-food",
                Description = "Burgers, wraps, rolls, and quick bites made at home.",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222205"),
                Name = "BBQ & Grills",
                Slug = "bbq-grills",
                Description = "Charcoal-grilled kebabs, tikkas, and smoked meat specialties.",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222206"),
                Name = "Bakery & Cakes",
                Slug = "bakery-cakes",
                Description = "Custom cakes, artisanal breads, pastries, and cookies.",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222207"),
                Name = "Desserts & Sweets",
                Slug = "desserts-sweets",
                Description = "Decadent puddings, traditional mithai, brownies, and sweet treats.",
                DisplayOrder = 7,
                IsActive = true,
                CreatedAtUtc = seedDate,
            },
            new Cuisine
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222208"),
                Name = "Continental",
                Slug = "continental",
                Description = "Western-style pasta, steaks, salads, and continental dishes.",
                DisplayOrder = 8,
                IsActive = true,
                CreatedAtUtc = seedDate,
            }
        );
    }
}
