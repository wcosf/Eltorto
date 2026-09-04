using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IOrderNotifier
{
    Task SendOrderCreatedAsync(OrderDto order, string toEmail, IReadOnlyList<string> imageUrls, CancellationToken cancellationToken = default);
}