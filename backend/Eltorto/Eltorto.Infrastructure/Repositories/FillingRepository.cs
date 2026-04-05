using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class FillingRepository : Repository<Filling>, IFillingRepository
{
    public FillingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Filling?> GetWithCakesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.Cakes)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Filling>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }
}