using HomeChef.Domain.Favorites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FavoriteFoodConfiguration : IEntityTypeConfiguration<FavoriteFood>
{
    public void Configure(EntityTypeBuilder<FavoriteFood> builder)
    {
        builder.ToTable("FavoriteFoods");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.FoodItem)
            .WithMany()
            .HasForeignKey(f => f.FoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.UserId, f.FoodItemId })
            .IsUnique();

        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.FoodItemId);
    }
}
