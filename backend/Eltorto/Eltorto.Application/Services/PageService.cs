using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Application.Services;

public class PageService : IPageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PageDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.Pages.GetWithBlocksAsync(slug, cancellationToken);
        return page != null ? _mapper.Map<PageDto>(page) : null;
    }

    public async Task<PageDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(id, cancellationToken);
        if (page == null)
            return null;

        var contentBlocks = await _unitOfWork.ContentBlocks.GetOrderedByPageAsync(id, cancellationToken);
        var pageDto = _mapper.Map<PageDto>(page);
        pageDto.ContentBlocks = _mapper.Map<List<ContentBlockDto>>(contentBlocks);

        return pageDto;
    }

    public async Task<IReadOnlyList<PageDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pages = await _unitOfWork.Pages.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<PageDto>>(pages);
    }

    public async Task<PageDto> UpdateAsync(int id, UpdatePageDto updateDto, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(id, cancellationToken);
        if (page == null)
        {
            throw new KeyNotFoundException($"Page with id {id} not found");
        }

        _mapper.Map(updateDto, page);
        await _unitOfWork.Pages.UpdateAsync(page, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<PageDto>(page);
    }

    public async Task<ContentBlockDto> AddContentBlockAsync(int pageId, CreateContentBlockDto createDto, CancellationToken cancellationToken = default)
    {
        var page = await _unitOfWork.Pages.GetByIdAsync(pageId, cancellationToken);
        if (page == null)
        {
            throw new KeyNotFoundException($"Page with id {pageId} not found");
        }

        if (await _unitOfWork.ContentBlocks.ExistsOnPageAsync(pageId, createDto.Title, cancellationToken))
        {
            throw new InvalidOperationException($"Content block with title '{createDto.Title}' already exists on this page");
        }

        if (createDto.SortOrder == 0)
        {
            var maxOrder = await _unitOfWork.ContentBlocks.GetMaxSortOrderAsync(pageId, cancellationToken);
            createDto.SortOrder = maxOrder + 1;
        }

        var contentBlock = _mapper.Map<ContentBlock>(createDto);
        contentBlock.PageId = pageId;

        await _unitOfWork.ContentBlocks.AddAsync(contentBlock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ContentBlockDto>(contentBlock);
    }

    public async Task<ContentBlockDto> UpdateContentBlockAsync(int blockId, UpdateContentBlockDto updateDto, CancellationToken cancellationToken = default)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(blockId, cancellationToken);
        if (contentBlock == null)
        {
            throw new KeyNotFoundException($"Content block with id {blockId} not found");
        }

        if (updateDto.Title != contentBlock.Title)
        {
            var exists = await _unitOfWork.ContentBlocks.ExistsOnPageAsync(contentBlock.PageId, updateDto.Title, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Content block with title '{updateDto.Title}' already exists on this page");
            }
        }

        _mapper.Map(updateDto, contentBlock);
        await _unitOfWork.ContentBlocks.UpdateAsync(contentBlock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ContentBlockDto>(contentBlock);
    }

    public async Task DeleteContentBlockAsync(int blockId, CancellationToken cancellationToken = default)
    {
        var contentBlock = await _unitOfWork.ContentBlocks.GetByIdAsync(blockId, cancellationToken);
        if (contentBlock == null)
        {
            throw new KeyNotFoundException($"Content block with id {blockId} not found");
        }

        await _unitOfWork.ContentBlocks.DeleteAsync(contentBlock, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ContentBlockDto>> GetPageContentBlocksAsync(int pageId, CancellationToken cancellationToken = default)
    {
        var contentBlocks = await _unitOfWork.ContentBlocks.GetOrderedByPageAsync(pageId, cancellationToken);
        return _mapper.Map<IReadOnlyList<ContentBlockDto>>(contentBlocks);
    }

    public async Task ReorderContentBlocksAsync(int pageId, List<int> orderedIds, CancellationToken cancellationToken = default)
    {
        var contentBlocks = await _unitOfWork.ContentBlocks.GetByPageIdAsync(pageId, cancellationToken);
        var blocksDict = contentBlocks.ToDictionary(b => b.Id);

        for (int i = 0; i < orderedIds.Count; i++)
        {
            if (blocksDict.TryGetValue(orderedIds[i], out var block))
            {
                block.SortOrder = i;
                await _unitOfWork.ContentBlocks.UpdateAsync(block, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}