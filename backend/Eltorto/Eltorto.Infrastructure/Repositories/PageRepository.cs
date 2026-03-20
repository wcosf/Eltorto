using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class PageRepository : Repository<Page>, IPageRepository
{
    public PageRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Page?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<Page?> GetWithBlocksAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ContentBlocks.OrderBy(b => b.SortOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }
}