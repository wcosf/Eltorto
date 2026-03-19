using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Cake> Cakes { get; set; }
    public DbSet<Filling> Fillings { get; set; }
    public DbSet<Testimonial> Testimonials { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<ContentBlock> ContentBlocks { get; set; }
    public DbSet<SliderItem> SliderItems { get; set; }
    public DbSet<ContactSettings> ContactSettings { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=eltorto_pg;Username=postgres;Password=password",
                npgsqlOptions => npgsqlOptions.SetPostgresVersion(16, 0)
                    .MigrationsAssembly("Eltorto.Infrastructure")); 
        }
    }
}