using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class FillingsController : BaseApiController
{
    private readonly IFillingService _fillingService;
    private readonly ILogger<FillingsController> _logger;

    public FillingsController(IFillingService fillingService, ILogger<FillingsController> logger)
    {
        _fillingService = fillingService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FillingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var fillings = await _fillingService.GetAllAsync(cancellationToken);
        return Ok(fillings);
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<FillingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
    {
        var fillings = await _fillingService.GetAvailableAsync(cancellationToken);
        return Ok(fillings);
    }

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

    [HttpPost]
    [ProducesResponseType(typeof(FillingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFillingDto createDto, CancellationToken cancellationToken)
    {
        var filling = await _fillingService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = filling.Id }, filling);
    }

    [HttpPut("{id:int}")]
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
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
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
}