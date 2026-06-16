using Eltorto.Domain.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Cake)
            .Include(o => o.Filling)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
    public async Task<IReadOnlyList<Order>> GetByCustomerAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Cake)
            .Include(o => o.Filling)
            .Where(o => o.CustomerPhone == phone)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(o => o.Cake)
            .Include(o => o.Filling)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPagedAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(o => o.Cake)
            .Include(o => o.Filling)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(o => o.Status == status);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var order = await GetByIdAsync(id, cancellationToken);
        if (order != null)
        {
            order.Status = status;
            await UpdateAsync(order, cancellationToken);
        }
    }
}