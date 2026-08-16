using HomeChef.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews", t =>
        {
            t.HasCheckConstraint("CK_Reviews_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(r => r.UpdatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasOne(r => r.ChefProfile)
            .WithMany()
            .HasForeignKey(r => r.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.CustomerUser)
            .WithMany()
            .HasForeignKey(r => r.CustomerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // One review per customer per chef
        builder.HasIndex(r => new { r.ChefProfileId, r.CustomerUserId })
            .IsUnique();

        builder.HasIndex(r => r.ChefProfileId);
        builder.HasIndex(r => r.CustomerUserId);
        builder.HasIndex(r => r.CreatedAtUtc);
    }
}
