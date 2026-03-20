using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Infrastructure.Data;
using Eltorto.Infrastructure.DataMigration;
using Eltorto.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eltorto.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Регистрация DbContext
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(3);
                npgsqlOptions.CommandTimeout(30);
                npgsqlOptions.SetPostgresVersion(16, 0);
            });

#if DEBUG
            options.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#endif
        });

        // Регистрация репозиториев
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ICakeRepository, CakeRepository>();
        services.AddScoped<IFillingRepository, FillingRepository>();
        services.AddScoped<ITestimonialRepository, TestimonialRepository>();
        services.AddScoped<IPageRepository, PageRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISliderRepository, SliderRepository>();

        // Регистрация UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Регистрация миграций данных
        services.AddSingleton<MigrationRunner>();

        return services;
    }

    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public static async Task MigrateDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
        await migrationRunner.RunMigrationsAsync();
    }
}