using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Slug");

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Title");

        builder.Property(e => e.MetaDescription)
            .HasMaxLength(500)
            .HasColumnName("MetaDescription");

        builder.Property(e => e.Heading)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Heading");

        builder.Property(e => e.Subheading)
            .HasMaxLength(500)
            .HasColumnName("Subheading");

        builder.Property(e => e.Content)
            .HasColumnType("text")
            .HasColumnName("Content");

        // Связь с блоками контента
        builder.HasMany(e => e.ContentBlocks)
            .WithOne(e => e.Page)
            .HasForeignKey(e => e.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Pages_Slug");
    }
}