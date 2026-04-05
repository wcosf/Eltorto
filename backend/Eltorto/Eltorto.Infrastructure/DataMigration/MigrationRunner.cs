using Dapper;
using Eltorto.Infrastructure.DataMigration.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Eltorto.Infrastructure.DataMigration;

public class MigrationRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationRunner> _logger;
    private readonly string _postgresConnectionString;
    private readonly string _mysqlConnectionString;

    public MigrationRunner(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<MigrationRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _postgresConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("PostgreSQL connection string not found");
        _mysqlConnectionString = configuration.GetConnectionString("MySql")
            ?? throw new InvalidOperationException("MySQL connection string not found");
    }

    public async Task RunMigrationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for pending data migrations...");

        var appliedMigrations = await GetAppliedMigrationsAsync(cancellationToken);

        var migrations = new List<IMigration>
        {
            new Migration_2026_03_20_InitialData(
                _mysqlConnectionString,
                _postgresConnectionString,
                _serviceProvider.GetRequiredService<ILogger<Migration_2026_03_20_InitialData>>())
        };

        foreach (var migration in migrations.OrderBy(m => m.Order))
        {
            if (!appliedMigrations.Contains(migration.Name))
            {
                _logger.LogInformation("Running migration: {MigrationName}", migration.Name);
                await migration.UpAsync(cancellationToken);
                _logger.LogInformation("Migration completed: {MigrationName}", migration.Name);
            }
            else
            {
                _logger.LogDebug("Migration already applied: {MigrationName}", migration.Name);
            }
        }

        _logger.LogInformation("All data migrations completed!");
    }

    private async Task<HashSet<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken)
    {
        using var postgresConn = new NpgsqlConnection(_postgresConnectionString);
        await postgresConn.OpenAsync(cancellationToken);

        await postgresConn.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ""__MigrationsHistory"" (
                ""Id"" SERIAL PRIMARY KEY,
                ""MigrationName"" VARCHAR(255) NOT NULL,
                ""AppliedDate"" TIMESTAMP NOT NULL,
                ""Details"" TEXT
            );");

        var migrations = await postgresConn.QueryAsync<string>(@"
            SELECT ""MigrationName"" FROM ""__MigrationsHistory""
            ORDER BY ""AppliedDate""");

        return new HashSet<string>(migrations);
    }
}