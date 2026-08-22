using HomeChef.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ChefMessageConfiguration : IEntityTypeConfiguration<ChefMessage>
{
    public void Configure(EntityTypeBuilder<ChefMessage> builder)
    {
        builder.ToTable("ChefMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(m => m.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(m => m.ReadAtUtc)
            .HasColumnType("timestamptz");

        builder.HasOne(m => m.ChefProfile)
            .WithMany()
            .HasForeignKey(m => m.ChefProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.ChefProfileId, m.CreatedAtUtc });
        builder.HasIndex(m => new { m.SenderUserId, m.CreatedAtUtc });
        builder.HasIndex(m => m.ChefProfileId);
        builder.HasIndex(m => m.SenderUserId);
    }
}
