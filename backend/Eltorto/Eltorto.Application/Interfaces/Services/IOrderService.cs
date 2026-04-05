using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByCustomerPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<PagedResultDto<OrderDto>> GetPagedAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateAsync(CreateOrderDto createDto, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}