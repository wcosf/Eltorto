using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class PagesController : BaseApiController
{
    private readonly IPageService _pageService;
    private readonly ILogger<PagesController> _logger;

    public PagesController(IPageService pageService, ILogger<PagesController> logger)
    {
        _pageService = pageService;
        _logger = logger;
    }

    // ========== PAGE ENDPOINTS ==========

    /// <summary>
    /// Get page by slug (for public access)
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var page = await _pageService.GetBySlugAsync(slug, cancellationToken);
        if (page == null)
            return NotFound();

        return Ok(page);
    }

    /// <summary>
    /// Get page by id (for admin)
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var page = await _pageService.GetByIdAsync(id, cancellationToken);
        if (page == null)
            return NotFound();

        return Ok(page);
    }

    /// <summary>
    /// Get all pages
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PageDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var pages = await _pageService.GetAllAsync(cancellationToken);
        return Ok(pages);
    }

    /// <summary>
    /// Update page content (Admin only)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePageDto updateDto, CancellationToken cancellationToken)
    {
        try
        {
            var page = await _pageService.UpdateAsync(id, updateDto, cancellationToken);
            return Ok(page);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ========== CONTENT BLOCK ENDPOINTS ==========

    /// <summary>
    /// Get all content blocks for a page
    /// </summary>
    [HttpGet("{pageId:int}/blocks")]
    [ProducesResponseType(typeof(IEnumerable<ContentBlockDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlocks(int pageId, CancellationToken cancellationToken)
    {
        var blocks = await _pageService.GetPageContentBlocksAsync(pageId, cancellationToken);
        return Ok(blocks);
    }

    /// <summary>
    /// Add content block to page (Admin only)
    /// </summary>
    [HttpPost("{pageId:int}/blocks")]
    [ProducesResponseType(typeof(ContentBlockDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddBlock(int pageId, [FromBody] CreateContentBlockDto createDto, CancellationToken cancellationToken)
    {
        try
        {
            var block = await _pageService.AddContentBlockAsync(pageId, createDto, cancellationToken);
            return CreatedAtAction(nameof(GetBlockById), new { blockId = block.Id }, block);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get content block by id
    /// </summary>
    [HttpGet("blocks/{blockId:int}")]
    [ProducesResponseType(typeof(ContentBlockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlockById(int blockId, CancellationToken cancellationToken)
    {
        // We need to get a single block - might want to add this method to service
        var page = await _pageService.GetByIdAsync(1, cancellationToken); // Temporary
        if (page == null)
            return NotFound();

        var block = page.ContentBlocks.FirstOrDefault(b => b.Id == blockId);
        if (block == null)
            return NotFound();

        return Ok(block);
    }

    /// <summary>
    /// Update content block (Admin only)
    /// </summary>
    [HttpPut("blocks/{blockId:int}")]
    [ProducesResponseType(typeof(ContentBlockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBlock(int blockId, [FromBody] UpdateContentBlockDto updateDto, CancellationToken cancellationToken)
    {
        if (blockId != updateDto.Id)
            return BadRequest(new { error = "Id mismatch" });

        try
        {
            var block = await _pageService.UpdateContentBlockAsync(blockId, updateDto, cancellationToken);
            return Ok(block);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Delete content block (Admin only)
    /// </summary>
    [HttpDelete("blocks/{blockId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBlock(int blockId, CancellationToken cancellationToken)
    {
        try
        {
            await _pageService.DeleteContentBlockAsync(blockId, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Reorder content blocks (Admin only)
    /// </summary>
    [HttpPost("{pageId:int}/blocks/reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReorderBlocks(int pageId, [FromBody] List<int> orderedIds, CancellationToken cancellationToken)
    {
        await _pageService.ReorderContentBlocksAsync(pageId, orderedIds, cancellationToken);
        return NoContent();
    }
}