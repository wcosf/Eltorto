using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Eltorto.Application.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _basePath;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _basePath = _configuration["UploadSettings:BasePath"] ?? "storage";
    }

    public async Task<string> SaveFileAsync(IFormFile file, string category, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            throw new ArgumentException("Invalid file format. Allowed: jpg, jpeg, png, gif, webp");

        if (file.Length > 5 * 1024 * 1024)
            throw new ArgumentException("File too large (max 5 MB)");

        var originalName = Path.GetFileNameWithoutExtension(file.FileName);
        var safeName = string.Join("_", originalName.Split(Path.GetInvalidFileNameChars()));
        if (string.IsNullOrEmpty(safeName))
            safeName = "image";

        var categoryPath = Path.Combine(_basePath, category);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), categoryPath);

        if (!Directory.Exists(fullPath))
            Directory.CreateDirectory(fullPath);

        var finalFileName = GenerateUniqueFileName(fullPath, safeName, ext);

        var filePath = Path.Combine(fullPath, finalFileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return finalFileName;
    }

    public string GetFileUrl(string fileName, string category)
    {
        if (string.IsNullOrEmpty(fileName))
            return string.Empty;

        var encoded = Uri.EscapeDataString(fileName);
        return $"/storage/{category}/{encoded}";
    }

    public async Task DeleteFileAsync(string fileName, string category, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileName))
            return;

        var categoryPath = Path.Combine(_basePath, category);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), categoryPath, fileName);

        if (File.Exists(fullPath))
        {
            await Task.Run(() => File.Delete(fullPath), cancellationToken);
            _logger.LogInformation("File deleted: {FilePath}", fullPath);
        }
    }

    private string GenerateUniqueFileName(string directory, string baseName, string extension)
    {
        var candidate = baseName + extension;
        var counter = 1;
        while (File.Exists(Path.Combine(directory, candidate)))
        {
            candidate = $"{baseName} ({counter}){extension}";
            counter++;
        }
        return candidate;
    }
}