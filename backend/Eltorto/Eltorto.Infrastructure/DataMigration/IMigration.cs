namespace Eltorto.Infrastructure.DataMigration;

public interface IMigration
{
    string Name { get; }
    int Order { get; }
    Task UpAsync(CancellationToken cancellationToken = default);
    Task DownAsync(CancellationToken cancellationToken = default);
}