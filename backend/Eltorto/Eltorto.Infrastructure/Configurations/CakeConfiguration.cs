using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class CakeConfiguration : IEntityTypeConfiguration<Cake>
{
    public void Configure(EntityTypeBuilder<Cake> builder)
    {
        builder.ToTable("Cakes");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Name");

        builder.Property(e => e.ImageUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("ImageUrl");

        builder.Property(e => e.ThumbnailUrl)
            .IsRequired()
            .HasMaxLength(500)
            .HasColumnName("ThumbnailUrl");

        builder.Property(e => e.CategorySlug)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("CategorySlug");

        builder.Property(e => e.SubCategory)
            .HasMaxLength(100)
            .HasColumnName("SubCategory");

        builder.Property(e => e.IsFeatured)
            .HasColumnName("IsFeatured");

        builder.Property(e => e.Description)
            .HasMaxLength(2000)
            .HasColumnName("Description");

        // Внешние ключи
        builder.HasOne(e => e.Filling)
            .WithMany(e => e.Cakes)
            .HasForeignKey(e => e.FillingId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Category>()
            .WithMany(e => e.Cakes)
            .HasForeignKey(e => e.CategorySlug)
            .HasPrincipalKey(e => e.Slug)
            .OnDelete(DeleteBehavior.Restrict);

        // Индексы
        builder.HasIndex(e => e.CategorySlug)
            .HasDatabaseName("IX_Cakes_CategorySlug");

        builder.HasIndex(e => e.IsFeatured)
            .HasDatabaseName("IX_Cakes_IsFeatured");

        builder.HasIndex(e => e.FillingId)
            .HasDatabaseName("IX_Cakes_FillingId");
    }
}