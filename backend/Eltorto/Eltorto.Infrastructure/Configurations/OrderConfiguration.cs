using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasColumnName("CreatedAt");

        builder.Property(e => e.CustomerName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("CustomerName");

        builder.Property(e => e.CustomerPhone)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("CustomerPhone");

        builder.Property(e => e.CustomerEmail)
            .HasMaxLength(200)
            .HasColumnName("CustomerEmail");

        builder.Property(e => e.CustomCakeDescription)
            .HasMaxLength(2000)
            .HasColumnName("CustomCakeDescription");

        builder.Property(e => e.Weight)
            .HasPrecision(10, 2)
            .HasColumnName("Weight");

        builder.Property(e => e.DeliveryDate)
            .HasColumnName("DeliveryDate");

        builder.Property(e => e.DeliveryAddress)
            .HasMaxLength(500)
            .HasColumnName("DeliveryAddress");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnName("Status");

        builder.Property(e => e.Comment)
            .HasMaxLength(1000)
            .HasColumnName("Comment");

        // Внешние ключи
        builder.HasOne<Cake>()
            .WithMany()
            .HasForeignKey(e => e.CakeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Filling>()
            .WithMany()
            .HasForeignKey(e => e.FillingId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Orders_Status");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_Orders_CreatedAt");

        builder.HasIndex(e => e.DeliveryDate)
            .HasDatabaseName("IX_Orders_DeliveryDate");

        builder.HasIndex(e => e.CustomerPhone)
            .HasDatabaseName("IX_Orders_CustomerPhone");
    }
}