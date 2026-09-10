using HomeChef.Domain.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeChef.Infrastructure.Configurations;

public sealed class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("ContactMessages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cm => cm.Phone)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(cm => cm.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(cm => cm.Message)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(cm => cm.IsRead)
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(cm => cm.Status)
            .HasMaxLength(50)
            .HasDefaultValue("New")
            .IsRequired();

        builder.Property(cm => cm.CreatedAtUtc)
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(cm => cm.ReadAtUtc)
            .HasColumnType("timestamptz");

        builder.HasIndex(cm => cm.IsRead);
        builder.HasIndex(cm => cm.Status);
        builder.HasIndex(cm => cm.CreatedAtUtc);
    }
}
