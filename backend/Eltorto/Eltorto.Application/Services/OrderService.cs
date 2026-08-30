using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Domain.Abstractions;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Eltorto.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public OrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OrderService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<OrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByCustomerPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerAsync(phone, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<IReadOnlyList<OrderDto>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByStatusAsync(status, cancellationToken);
        return _mapper.Map<IReadOnlyList<OrderDto>>(orders);
    }

    public async Task<PagedResultDto<OrderDto>> GetPagedAsync(int page, int pageSize, string? status = null, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetPagedAsync(page, pageSize, status, cancellationToken);
        var totalCount = status != null
            ? await _unitOfWork.Orders.CountAsync(o => o.Status == status, cancellationToken)
            : await _unitOfWork.Orders.CountAsync(cancellationToken);

        return new PagedResultDto<OrderDto>
        {
            Items = _mapper.Map<List<OrderDto>>(orders),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto createDto, CancellationToken cancellationToken = default)
    {
        var order = _mapper.Map<Order>(createDto);

        if (order.DeliveryDate.HasValue)
        {
            order.DeliveryDate = DateTime.SpecifyKind(order.DeliveryDate.Value, DateTimeKind.Utc);
        }

        if (createDto.CakeId.HasValue)
        {
            var cakeExists = await _unitOfWork.Cakes.ExistsAsync(c => c.Id == createDto.CakeId.Value, cancellationToken);
            if (!cakeExists)
            {
                throw new InvalidOperationException($"Cake with id {createDto.CakeId} does not exist");
            }
        }

        if (createDto.FillingId.HasValue)
        {
            var fillingExists = await _unitOfWork.Fillings.ExistsAsync(f => f.Id == createDto.FillingId.Value, cancellationToken);
            if (!fillingExists)
            {
                throw new InvalidOperationException($"Filling with id {createDto.FillingId} does not exist");
            }
        }

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdOrder = await _unitOfWork.Orders.GetByIdAsync(order.Id, cancellationToken);
        var result = _mapper.Map<OrderDto>(createdOrder);

        using (ExecutionContext.SuppressFlow())
        {
            _ = Task.Run(() => NotifyOrderCreatedAsync(result));
        }

        return result;
    }

    private async Task NotifyOrderCreatedAsync(OrderDto order)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            var contact = await serviceProvider.GetRequiredService<IContactSettingsService>().GetAsync();
            var toEmail = contact?.Email;
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return;
            }

            var imageUrls = BuildOrderImageUrls(
                order,
                serviceProvider.GetRequiredService<ISiteUrlProvider>(),
                serviceProvider.GetRequiredService<IFileStorageService>());

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            try
            {
                var notifier = serviceProvider.GetRequiredService<IOrderNotifier>();
                await notifier.SendOrderCreatedAsync(order, toEmail, imageUrls, cts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("[EMAIL] Order notification send was canceled (timeout) for order {OrderId}", order.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[EMAIL] Failed to send order notification for order {OrderId}", order.Id);
        }
    }

    private static IReadOnlyList<string> BuildOrderImageUrls(OrderDto order, ISiteUrlProvider siteUrlProvider, IFileStorageService fileStorageService)
    {
        var baseUrl = siteUrlProvider.GetBaseUrl();
        var urls = new List<string>(2);

        var cakePath = IsHttpUrl(order.CakeImageUrl)
            ? order.CakeImageUrl
            : fileStorageService.GetFileUrl(order.CakeImageUrl, "cakes");
        if (ToAbsoluteUrl(baseUrl, cakePath) is { } cakeUrl)
        {
            urls.Add(cakeUrl);
        }

        var fillingPath = IsHttpUrl(order.FillingImageUrl)
            ? order.FillingImageUrl
            : fileStorageService.GetFileUrl(order.FillingImageUrl, "fillings");
        if (ToAbsoluteUrl(baseUrl, fillingPath) is { } fillingUrl)
        {
            urls.Add(fillingUrl);
        }

        return urls;
    }

    private static bool IsHttpUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url)
            && Uri.TryCreate(url, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps);
    }

    private static string? ToAbsoluteUrl(string baseUrl, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        if (IsHttpUrl(path))
        {
            return path;
        }

        return string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl + path;
    }

    public async Task<OrderDto> UpdateStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        var validStatuses = new[] { "New", "Processing", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            throw new InvalidOperationException($"Invalid status. Allowed values: {string.Join(", ", validStatuses)}");
        }

        order.Status = status;
        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> UpdateAsync(int id, CreateOrderDto updateDto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        if (updateDto.CakeId.HasValue)
        {
            var cakeExists = await _unitOfWork.Cakes.ExistsAsync(c => c.Id == updateDto.CakeId.Value, cancellationToken);
            if (!cakeExists)
            {
                throw new InvalidOperationException($"Cake with id {updateDto.CakeId} does not exist");
            }
        }

        if (updateDto.FillingId.HasValue)
        {
            var fillingExists = await _unitOfWork.Fillings.ExistsAsync(f => f.Id == updateDto.FillingId.Value, cancellationToken);
            if (!fillingExists)
            {
                throw new InvalidOperationException($"Filling with id {updateDto.FillingId} does not exist");
            }
        }

        order.CustomerName = updateDto.CustomerName;
        order.CustomerPhone = updateDto.CustomerPhone;
        order.CustomerEmail = updateDto.CustomerEmail;
        order.CakeId = updateDto.CakeId;
        order.CustomCakeDescription = updateDto.CustomCakeDescription;
        order.FillingId = updateDto.FillingId;
        order.Weight = updateDto.Weight;
        order.DeliveryDate = updateDto.DeliveryDate.HasValue
            ? DateTime.SpecifyKind(updateDto.DeliveryDate.Value, DateTimeKind.Utc)
            : updateDto.DeliveryDate;
        order.DeliveryAddress = updateDto.DeliveryAddress;
        order.Comment = updateDto.Comment;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with id {id} not found");
        }

        await _unitOfWork.Orders.DeleteAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}