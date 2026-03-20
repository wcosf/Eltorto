using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface ISliderRepository : IRepository<SliderItem>
{
    Task<IReadOnlyList<SliderItem>> GetOrderedAsync(CancellationToken cancellationToken = default);
}