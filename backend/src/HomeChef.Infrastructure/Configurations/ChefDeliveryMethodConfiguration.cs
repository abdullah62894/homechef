using HomeChef.Domain.Chefs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ChefDeliveryMethodConfiguration : IEntityTypeConfiguration<ChefDeliveryMethod>
{
    public void Configure(EntityTypeBuilder<ChefDeliveryMethod> builder)
    {
        builder.ToTable("ChefDeliveryMethods");

        builder.HasKey(dm => dm.Id);

        builder.HasOne(dm => dm.ChefProfile)
            .WithMany()
            .HasForeignKey(dm => dm.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(dm => dm.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(dm => dm.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.HasIndex(dm => dm.ChefProfileId);
        builder.HasIndex(dm => new { dm.ChefProfileId, dm.Method })
            .IsUnique();
    }
}
