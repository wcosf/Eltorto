using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Eltorto.Infrastructure.Repositories;

public class TestimonialRepository : Repository<Testimonial>, ITestimonialRepository
{
    public TestimonialRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Testimonial>> GetApprovedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsApproved)
            .OrderByDescending(t => t.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Testimonial>> GetLatestAsync(int count, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsApproved)
            .OrderByDescending(t => t.Date)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Testimonial>> GetPagedApprovedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.IsApproved)
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Testimonial>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetApprovedCountAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.CountAsync(t => t.IsApproved, cancellationToken);
    }
}