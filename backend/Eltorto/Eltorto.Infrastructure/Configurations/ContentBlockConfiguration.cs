using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class ContentBlockConfiguration : IEntityTypeConfiguration<ContentBlock>
{
    public void Configure(EntityTypeBuilder<ContentBlock> builder)
    {
        builder.ToTable("ContentBlocks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Title");

        builder.Property(e => e.Text)
            .HasColumnType("text")
            .HasColumnName("Text");

        builder.Property(e => e.ImageUrl)
            .HasMaxLength(500)
            .HasColumnName("ImageUrl");

        builder.Property(e => e.SortOrder)
            .HasColumnName("SortOrder");

        builder.Property(e => e.PageId)
            .HasColumnName("PageId");

        // Внешний ключ
        builder.HasOne(e => e.Page)
            .WithMany(e => e.ContentBlocks)
            .HasForeignKey(e => e.PageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(e => e.PageId)
            .HasDatabaseName("IX_ContentBlocks_PageId");

        builder.HasIndex(e => new { e.PageId, e.SortOrder })
            .HasDatabaseName("IX_ContentBlocks_PageId_SortOrder");
    }
}