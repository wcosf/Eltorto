using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
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

        return _mapper.Map<OrderDto>(order);
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