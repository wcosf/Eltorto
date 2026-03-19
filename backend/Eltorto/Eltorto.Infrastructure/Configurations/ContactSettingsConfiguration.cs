using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class ContactSettingsConfiguration : IEntityTypeConfiguration<ContactSettings>
{
    public void Configure(EntityTypeBuilder<ContactSettings> builder)
    {
        builder.ToTable("ContactSettings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Phone)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("Phone");

        builder.Property(e => e.AdditionalPhone)
            .HasMaxLength(50)
            .HasColumnName("AdditionalPhone");

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Email");

        builder.Property(e => e.Address)
            .HasMaxLength(500)
            .HasColumnName("Address");

        builder.Property(e => e.MapUrl)
            .HasMaxLength(1000)
            .HasColumnName("MapUrl");

        // Гарантируем только одну запись
        builder.HasIndex(e => e.Id)
            .HasDatabaseName("IX_ContactSettings_Single")
            .IsUnique();
    }
}
