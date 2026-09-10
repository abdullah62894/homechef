using HomeChef.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.HasOne(o => o.CustomerUser)
            .WithMany()
            .HasForeignKey(o => o.CustomerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.ChefProfile)
            .WithMany()
            .HasForeignKey(o => o.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.Currency)
            .HasMaxLength(10)
            .HasDefaultValue("PKR")
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.DeliveryAddress)
            .HasMaxLength(500);

        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(30);

        builder.Property(o => o.CustomerName)
            .HasMaxLength(200);

        builder.Property(o => o.DeliveryMethod)
            .HasMaxLength(50);

        builder.Property(o => o.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(o => o.WhatsAppInitiatedAtUtc)
            .HasColumnType("timestamptz");

        builder.Property(o => o.CompletedAtUtc)
            .HasColumnType("timestamptz");

        builder.Property(o => o.CancelledAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(o => o.CustomerUserId);
        builder.HasIndex(o => o.ChefProfileId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAtUtc);
        builder.HasIndex(o => new { o.ChefProfileId, o.Status });
    }
}
