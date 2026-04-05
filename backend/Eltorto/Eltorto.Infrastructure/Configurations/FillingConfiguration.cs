using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class FillingConfiguration : IEntityTypeConfiguration<Filling>
{
    public void Configure(EntityTypeBuilder<Filling> builder)
    {
        builder.ToTable("Fillings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Name");

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .HasColumnName("Description");

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("ImageUrl");

        builder.Property(e => e.HasCrossSection)
            .HasColumnName("HasCrossSection");

        // Индексы
        builder.HasIndex(e => e.Name)
            .HasDatabaseName("IX_Fillings_Name");
    }
}