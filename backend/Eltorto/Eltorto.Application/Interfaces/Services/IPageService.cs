using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IPageService
{
    Task<PageDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<PageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PageDto> UpdateAsync(int id, UpdatePageDto updateDto, CancellationToken cancellationToken = default);

    // Content Block methods
    Task<ContentBlockDto> AddContentBlockAsync(int pageId, CreateContentBlockDto createDto, CancellationToken cancellationToken = default);
    Task<ContentBlockDto> UpdateContentBlockAsync(int blockId, UpdateContentBlockDto updateDto, CancellationToken cancellationToken = default);
    Task DeleteContentBlockAsync(int blockId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ContentBlockDto>> GetPageContentBlocksAsync(int pageId, CancellationToken cancellationToken = default);
    Task ReorderContentBlocksAsync(int pageId, List<int> orderedIds, CancellationToken cancellationToken = default);
}