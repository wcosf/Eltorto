using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Slug == slug, cancellationToken);
    }
}