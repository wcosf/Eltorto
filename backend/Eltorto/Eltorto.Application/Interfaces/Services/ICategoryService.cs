using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryWithCakesDto> GetWithCakesAsync(string slug, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateAsync(CreateCategoryDto createDto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateAsync(UpdateCategoryDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}