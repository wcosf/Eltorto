using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class SliderItemConfiguration : IEntityTypeConfiguration<SliderItem>
{
    public void Configure(EntityTypeBuilder<SliderItem> builder)
    {
        builder.ToTable("SliderItems");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("ImageUrl");

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .HasColumnName("Title");

        builder.Property(e => e.Subtitle)
            .HasMaxLength(500)
            .HasColumnName("Subtitle");

        builder.Property(e => e.SortOrder)
            .HasColumnName("SortOrder");

        // Индексы
        builder.HasIndex(e => e.SortOrder)
            .HasDatabaseName("IX_SliderItems_SortOrder");
    }
}