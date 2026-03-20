using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class SliderRepository : Repository<SliderItem>, ISliderRepository
{
    public SliderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<SliderItem>> GetOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .OrderBy(s => s.SortOrder)
            .ToListAsync(cancellationToken);
    }
}