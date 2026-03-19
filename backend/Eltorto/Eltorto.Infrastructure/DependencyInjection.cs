using Eltorto.Infrastructure.Data;
using Eltorto.Infrastructure.DataMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;

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
            });

#if DEBUG
            options.LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
#endif
        });

        // Регистрация миграций данных
        services.AddSingleton<MigrationRunner>();

        // Регистрация репозиториев (если будете использовать)
        // services.AddScoped<ICategoryRepository, CategoryRepository>();
        // services.AddScoped<ICakeRepository, CakeRepository>();

        return services;
    }

    // Метод для применения миграций при старте
    public static async Task ApplyMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Применяем EF Core миграции (структура БД)
        await dbContext.Database.MigrateAsync();
    }

    // Метод для запуска миграции данных
    public static async Task MigrateDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();

        // Запускаем миграцию данных из MySQL
        await migrationRunner.RunMigrationsAsync();
    }
}