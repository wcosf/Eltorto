using Eltorto.Domain.Entities;

namespace Eltorto.Domain.Repositories;

public interface ISliderRepository : IRepository<SliderItem>
{
    Task<IReadOnlyList<SliderItem>> GetOrderedAsync(CancellationToken cancellationToken = default);
}