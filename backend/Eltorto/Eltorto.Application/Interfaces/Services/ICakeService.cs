using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface ICakeService
{
    Task<CakeDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CakeListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CakeListDto>> GetByCategoryAsync(string categorySlug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CakeListDto>> GetFeaturedAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CakeListDto>> GetByFillingAsync(int fillingId, CancellationToken cancellationToken = default);
    Task<PagedResultDto<CakeListDto>> GetPagedAsync(int page, int pageSize, string? category = null, CancellationToken cancellationToken = default);
    Task<CakeDetailDto> CreateAsync(CreateCakeDto createDto, CancellationToken cancellationToken = default);
    Task<CakeDetailDto> UpdateAsync(UpdateCakeDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}