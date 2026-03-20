using Eltorto.Domain.Entities;

namespace Eltorto.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IReadOnlyList<Order>> GetByCustomerAsync(string phone, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetPagedAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
}