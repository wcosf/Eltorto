using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Eltorto.Infrastructure.Repositories;

public class ContentBlockRepository : Repository<ContentBlock>, IContentBlockRepository
{
    public ContentBlockRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ContentBlock>> GetByPageIdAsync(int pageId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cb => cb.PageId == pageId)
            .OrderBy(cb => cb.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentBlock>> GetOrderedByPageAsync(int pageId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cb => cb.PageId == pageId)
            .OrderBy(cb => cb.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsOnPageAsync(int pageId, string title, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(cb => cb.PageId == pageId && cb.Title == title, cancellationToken);
    }

    public async Task<int> GetMaxSortOrderAsync(int pageId, CancellationToken cancellationToken = default)
    {
        var maxOrder = await _dbSet
            .Where(cb => cb.PageId == pageId)
            .MaxAsync(cb => (int?)cb.SortOrder, cancellationToken);

        return maxOrder ?? -1;
    }
}