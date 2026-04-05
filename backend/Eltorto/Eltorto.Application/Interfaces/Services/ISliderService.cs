using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface ISliderService
{
    Task<SliderItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SliderItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SliderItemDto>> GetOrderedAsync(CancellationToken cancellationToken = default);
    Task<SliderItemDto> CreateAsync(CreateSliderItemDto createDto, CancellationToken cancellationToken = default);
    Task<SliderItemDto> UpdateAsync(UpdateSliderItemDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task ReorderAsync(List<int> orderedIds, CancellationToken cancellationToken = default);
}