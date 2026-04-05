using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class CakesController : BaseApiController
{
    private readonly ICakeService _cakeService;
    private readonly ILogger<CakesController> _logger;

    public CakesController(ICakeService cakeService, ILogger<CakesController> logger)
    {
        _cakeService = cakeService;
        _logger = logger;
    }

    /// <summary>
    /// Get all cakes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CakeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var cakes = await _cakeService.GetAllAsync(cancellationToken);
        return Ok(cakes);
    }

    /// <summary>
    /// Get paged cakes with optional category filter
    /// </summary>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(PagedResultDto<CakeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _cakeService.GetPagedAsync(page, pageSize, category, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get featured cakes
    /// </summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<CakeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured([FromQuery] int count = 6, CancellationToken cancellationToken = default)
    {
        var cakes = await _cakeService.GetFeaturedAsync(count, cancellationToken);
        return Ok(cakes);
    }

    /// <summary>
    /// Get cakes by category
    /// </summary>
    [HttpGet("by-category/{categorySlug}")]
    [ProducesResponseType(typeof(IEnumerable<CakeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string categorySlug, CancellationToken cancellationToken)
    {
        var cakes = await _cakeService.GetByCategoryAsync(categorySlug, cancellationToken);
        return Ok(cakes);
    }

    /// <summary>
    /// Get cakes by filling
    /// </summary>
    [HttpGet("by-filling/{fillingId:int}")]
    [ProducesResponseType(typeof(IEnumerable<CakeListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByFilling(int fillingId, CancellationToken cancellationToken)
    {
        var cakes = await _cakeService.GetByFillingAsync(fillingId, cancellationToken);
        return Ok(cakes);
    }

    /// <summary>
    /// Get cake by id
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CakeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var cake = await _cakeService.GetByIdAsync(id, cancellationToken);
        if (cake == null)
            return NotFound();

        return Ok(cake);
    }

    /// <summary>
    /// Create new cake (Admin only)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CakeDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCakeDto createDto, CancellationToken cancellationToken)
    {
        try
        {
            var cake = await _cakeService.CreateAsync(createDto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = cake.Id }, cake);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Update cake (Admin only)
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CakeDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCakeDto updateDto, CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return BadRequest(new { error = "Id mismatch" });

        try
        {
            var cake = await _cakeService.UpdateAsync(updateDto, cancellationToken);
            return Ok(cake);
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
    /// Delete cake (Admin only)
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _cakeService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}