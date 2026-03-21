using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class SliderController : BaseApiController
{
    private readonly ISliderService _sliderService;
    private readonly ILogger<SliderController> _logger;

    public SliderController(ISliderService sliderService, ILogger<SliderController> logger)
    {
        _sliderService = sliderService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SliderItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sliderItems = await _sliderService.GetOrderedAsync(cancellationToken);
        return Ok(sliderItems);
    }

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

    [HttpPost]
    [ProducesResponseType(typeof(SliderItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSliderItemDto createDto, CancellationToken cancellationToken)
    {
        var sliderItem = await _sliderService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = sliderItem.Id }, sliderItem);
    }

    [HttpPut("{id:int}")]
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

    [HttpDelete("{id:int}")]
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

    [HttpPost("reorder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds, CancellationToken cancellationToken)
    {
        await _sliderService.ReorderAsync(orderedIds, cancellationToken);
        return NoContent();
    }
}