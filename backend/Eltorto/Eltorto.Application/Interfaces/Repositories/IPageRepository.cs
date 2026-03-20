using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface IPageRepository : IRepository<Page>
{
    Task<Page?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Page?> GetWithBlocksAsync(string slug, CancellationToken cancellationToken = default);
}