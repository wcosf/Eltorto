using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Eltorto.API.Controllers;

public class SliderController : BaseApiController
{
    private readonly ISliderService _sliderService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<SliderController> _logger;

    public SliderController(
        ISliderService sliderService,
        IFileStorageService fileStorage,
        ILogger<SliderController> logger)
    {
        _sliderService = sliderService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>Gets all slider items in order.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SliderItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sliderItems = await _sliderService.GetOrderedAsync(cancellationToken);
        return Ok(sliderItems);
    }

    /// <summary>Gets a slider item by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SliderItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var sliderItem = await _sliderService.GetByIdAsync(id, cancellationToken);
        if (sliderItem == null)
            return NotFound();

        return Ok(sliderItem);
    }

    /// <summary>Creates a new slider item.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SliderItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSliderItemDto createDto, CancellationToken cancellationToken)
    {
        var sliderItem = await _sliderService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sliderItem.Id }, sliderItem);
    }

    /// <summary>Updates a slider item.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SliderItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSliderItemDto updateDto, CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return BadRequest(new { error = "Id mismatch" });

        try
        {
            var sliderItem = await _sliderService.UpdateAsync(updateDto, cancellationToken);
            return Ok(sliderItem);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Deletes a slider item. (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _sliderService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Reorders slider items.</summary>
    [HttpPost("reorder")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds, CancellationToken cancellationToken)
    {
        await _sliderService.ReorderAsync(orderedIds, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Upload image for slider
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var fileName = await _fileStorage.SaveFileAsync(file, "slider");
            return Ok(new UploadResultDto { ImageUrl = fileName });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading slider image");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}