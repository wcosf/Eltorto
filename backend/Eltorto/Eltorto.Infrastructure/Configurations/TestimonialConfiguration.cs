using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eltorto.Infrastructure.Configurations;

public class TestimonialConfiguration : IEntityTypeConfiguration<Testimonial>
{
    public void Configure(EntityTypeBuilder<Testimonial> builder)
    {
        builder.ToTable("Testimonials");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Date)
            .IsRequired()
            .HasColumnName("Date");

        builder.Property(e => e.Author)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("Author");

        builder.Property(e => e.Email)
            .HasMaxLength(200)
            .HasColumnName("Email");

        builder.Property(e => e.Text)
            .IsRequired()
            .HasMaxLength(4000)
            .HasColumnName("Text");

        builder.Property(e => e.Response)
            .HasMaxLength(4000)
            .HasColumnName("Response");

        builder.Property(e => e.IsApproved)
            .HasColumnName("IsApproved");

        // Индексы
        builder.HasIndex(e => e.IsApproved)
            .HasDatabaseName("IX_Testimonials_IsApproved");

        builder.HasIndex(e => e.Date)
            .HasDatabaseName("IX_Testimonials_Date");
    }
}
