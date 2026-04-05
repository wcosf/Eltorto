using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface ICakeRepository : IRepository<Cake>
{
    Task<IReadOnlyList<Cake>> GetByCategoryAsync(string categorySlug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cake>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cake>> GetByFillingAsync(int fillingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cake>> GetPagedAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(string? category = null, CancellationToken cancellationToken = default);
}