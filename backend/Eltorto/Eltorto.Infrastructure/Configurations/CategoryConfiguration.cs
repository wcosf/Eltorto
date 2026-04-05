using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("Slug");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Name");

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .HasColumnName("Description");

        builder.Property(e => e.SortOrder)
            .HasColumnName("SortOrder");

        // Индексы
        builder.HasIndex(e => e.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Categories_Slug");

        builder.HasIndex(e => e.SortOrder)
            .HasDatabaseName("IX_Categories_SortOrder");
    }
}
