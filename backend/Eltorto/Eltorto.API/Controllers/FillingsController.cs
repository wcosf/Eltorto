using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Eltorto.API.Controllers;

public class FillingsController : BaseApiController
{
    private readonly IFillingService _fillingService;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FillingsController> _logger;

    public FillingsController(
        IFillingService fillingService,
        IFileStorageService fileStorage,
        ILogger<FillingsController> logger)
    {
        _fillingService = fillingService;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    /// <summary>Gets all fillings.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FillingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var fillings = await _fillingService.GetAllAsync(cancellationToken);
        return Ok(fillings);
    }

    /// <summary>Gets available fillings.</summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<FillingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
    {
        var fillings = await _fillingService.GetAvailableAsync(cancellationToken);
        return Ok(fillings);
    }

    /// <summary>Gets a filling by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FillingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var filling = await _fillingService.GetByIdAsync(id, cancellationToken);
        if (filling == null)
            return NotFound();

        return Ok(filling);
    }

    /// <summary>Gets a filling with its cakes.</summary>
    [HttpGet("{id:int}/with-cakes")]
    [ProducesResponseType(typeof(FillingWithCakesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithCakes(int id, CancellationToken cancellationToken)
    {
        try
        {
            var filling = await _fillingService.GetWithCakesAsync(id, cancellationToken);
            return Ok(filling);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Creates a new filling.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FillingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFillingDto createDto, CancellationToken cancellationToken)
    {
        var filling = await _fillingService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = filling.Id }, filling);
    }

    /// <summary>Updates a filling.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FillingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFillingDto updateDto, CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return BadRequest(new { error = "Id mismatch" });

        try
        {
            var filling = await _fillingService.UpdateAsync(updateDto, cancellationToken);
            return Ok(filling);
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

    /// <summary>Deletes a filling. (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            var filling = await _fillingService.GetByIdAsync(id, cancellationToken);
            if (filling != null && !string.IsNullOrEmpty(filling.ImageUrl))
            {
                await _fileStorage.DeleteFileAsync(filling.ImageUrl, "fillings", cancellationToken);
            }
            await _fillingService.DeleteAsync(id, cancellationToken);
            return NoContent();
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
    /// Upload an image for filling
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UploadResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] int? id = null)
    {
        try
        {
            if (id.HasValue)
            {
                var existing = await _fillingService.GetByIdAsync(id.Value, CancellationToken.None);
                if (existing != null && !string.IsNullOrEmpty(existing.ImageUrl))
                {
                    await _fileStorage.DeleteFileAsync(existing.ImageUrl, "fillings", CancellationToken.None);
                }
            }

            var fileName = await _fileStorage.SaveFileAsync(file, "fillings");
            return Ok(new UploadResultDto { ImageUrl = fileName });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading filling image");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Delete image for filling
    /// </summary>
    [HttpDelete("{id:int}/image")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
    {
        var filling = await _fillingService.GetByIdAsync(id, cancellationToken);
        if (filling == null)
            return NotFound();

        if (!string.IsNullOrEmpty(filling.ImageUrl))
        {
            await _fileStorage.DeleteFileAsync(filling.ImageUrl, "fillings", cancellationToken);
            await _fillingService.UpdateImageUrlAsync(id, string.Empty, cancellationToken);
        }

        return NoContent();
    }
}