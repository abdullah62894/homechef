using HomeChef.Domain.Favorites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class FavoriteChefConfiguration : IEntityTypeConfiguration<FavoriteChef>
{
    public void Configure(EntityTypeBuilder<FavoriteChef> builder)
    {
        builder.ToTable("FavoriteChefs");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ChefProfile)
            .WithMany()
            .HasForeignKey(f => f.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.UserId, f.ChefProfileId })
            .IsUnique();

        builder.HasIndex(f => f.UserId);
        builder.HasIndex(f => f.ChefProfileId);
    }
}
