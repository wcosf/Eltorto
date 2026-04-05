using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface IContentBlockRepository : IRepository<ContentBlock>
{
    Task<IReadOnlyList<ContentBlock>> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentBlock>> GetOrderedByPageAsync(int pageId, CancellationToken cancellationToken = default);
    Task<bool> ExistsOnPageAsync(int pageId, string title, CancellationToken cancellationToken = default);
    Task<int> GetMaxSortOrderAsync(int pageId, CancellationToken cancellationToken = default);
}