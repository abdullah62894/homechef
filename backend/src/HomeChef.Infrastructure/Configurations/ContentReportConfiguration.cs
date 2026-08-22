using HomeChef.Domain.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ContentReportConfiguration : IEntityTypeConfiguration<ContentReport>
{
    public void Configure(EntityTypeBuilder<ContentReport> builder)
    {
        builder.ToTable("ContentReports");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Targets use optional cascading FKs so moderation deletes of the
        // content automatically clean up its reports.
        builder.HasOne<HomeChef.Domain.Chefs.ChefProfile>()
            .WithMany()
            .HasForeignKey(r => r.TargetChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<HomeChef.Domain.Foods.FoodItem>()
            .WithMany()
            .HasForeignKey(r => r.TargetFoodItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<HomeChef.Domain.Reviews.Review>()
            .WithMany()
            .HasForeignKey(r => r.TargetReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.TargetType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Reason)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Details)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(r => r.ResolvedAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.ReporterUserId);
        builder.HasIndex(r => r.CreatedAtUtc);
        builder.HasIndex(r => new { r.ReporterUserId, r.Status });
    }
}
