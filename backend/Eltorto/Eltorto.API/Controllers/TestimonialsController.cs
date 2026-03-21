using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class TestimonialsController : BaseApiController
{
    private readonly ITestimonialService _testimonialService;
    private readonly ILogger<TestimonialsController> _logger;

    public TestimonialsController(ITestimonialService testimonialService, ILogger<TestimonialsController> logger)
    {
        _testimonialService = testimonialService;
        _logger = logger;
    }

    [HttpGet("approved")]
    [ProducesResponseType(typeof(IEnumerable<TestimonialListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApproved(CancellationToken cancellationToken)
    {
        var testimonials = await _testimonialService.GetApprovedAsync(cancellationToken);
        return Ok(testimonials);
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<TestimonialListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLatest([FromQuery] int count = 5, CancellationToken cancellationToken = default)
    {
        var testimonials = await _testimonialService.GetLatestAsync(count, cancellationToken);
        return Ok(testimonials);
    }

    [HttpGet("paged/approved")]
    [ProducesResponseType(typeof(PagedResultDto<TestimonialListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedApproved(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _testimonialService.GetPagedApprovedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("paged/all")]
    [ProducesResponseType(typeof(PagedResultDto<TestimonialListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _testimonialService.GetPagedAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialService.GetByIdAsync(id, cancellationToken);
        if (testimonial == null)
            return NotFound();

        return Ok(testimonial);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialDto createDto, CancellationToken cancellationToken)
    {
        var testimonial = await _testimonialService.CreateAsync(createDto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = testimonial.Id }, testimonial);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTestimonialDto updateDto, CancellationToken cancellationToken)
    {
        if (id != updateDto.Id)
            return BadRequest(new { error = "Id mismatch" });

        try
        {
            var testimonial = await _testimonialService.UpdateAsync(updateDto, cancellationToken);
            return Ok(testimonial);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:int}/approve")]
    [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(int id, CancellationToken cancellationToken)
    {
        try
        {
            var testimonial = await _testimonialService.ApproveAsync(id, cancellationToken);
            return Ok(testimonial);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPatch("{id:int}/response")]
    [ProducesResponseType(typeof(TestimonialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddResponse(int id, [FromBody] string response, CancellationToken cancellationToken)
    {
        try
        {
            var testimonial = await _testimonialService.AddResponseAsync(id, response, cancellationToken);
            return Ok(testimonial);
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
            await _testimonialService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}