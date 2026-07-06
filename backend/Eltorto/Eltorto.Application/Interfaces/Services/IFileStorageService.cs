using Microsoft.AspNetCore.Http;

namespace Eltorto.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string category, CancellationToken cancellationToken = default);
    string GetFileUrl(string fileName, string category);
    Task DeleteFileAsync(string fileName, string category, CancellationToken cancellationToken = default);
}