using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class CakeRepository : Repository<Cake>, ICakeRepository
{
    public CakeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Cake>> GetByCategoryAsync(string categorySlug, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filling)
            .Where(c => c.CategorySlug == categorySlug)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cake>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filling)
            .Where(c => c.IsFeatured)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cake>> GetByFillingAsync(int fillingId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Filling)
            .Where(c => c.FillingId == fillingId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Cake>> GetPagedAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(c => c.Filling).AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(c => c.CategorySlug == category);
        }

        return await query
            .OrderByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(string? category = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(c => c.CategorySlug == category);
        }

        return await query.CountAsync(cancellationToken);
    }
}