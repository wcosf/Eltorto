using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface IFillingRepository : IRepository<Filling>
{
    Task<Filling?> GetWithCakesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Filling>> GetAvailableAsync(CancellationToken cancellationToken = default);
}