namespace Eltorto.Infrastructure.DataMigration;

public interface IDataMigrator
{
    Task MigrateAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
