using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IContactSettingsService
{
    Task<ContactSettingsDto?> GetAsync(CancellationToken cancellationToken = default);
    Task<ContactSettingsDto> UpdateAsync(UpdateContactSettingsDto updateDto, CancellationToken cancellationToken = default);
}