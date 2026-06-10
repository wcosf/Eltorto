using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Data;

public static class DbContextOptionsConfigurator
{
    public static void ConfigureNpgsql(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        string migrationsAssembly)
    {
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(migrationsAssembly);
            npgsqlOptions.EnableRetryOnFailure(3);
            npgsqlOptions.CommandTimeout(30);
            npgsqlOptions.SetPostgresVersion(16, 0);
        });
    }
}