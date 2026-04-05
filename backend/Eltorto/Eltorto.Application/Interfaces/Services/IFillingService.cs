using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IFillingService
{
    Task<FillingDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FillingDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FillingDto>> GetAvailableAsync(CancellationToken cancellationToken = default);
    Task<FillingWithCakesDto> GetWithCakesAsync(int id, CancellationToken cancellationToken = default);
    Task<FillingDto> CreateAsync(CreateFillingDto createDto, CancellationToken cancellationToken = default);
    Task<FillingDto> UpdateAsync(UpdateFillingDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}