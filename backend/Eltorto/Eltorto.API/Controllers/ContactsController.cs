using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class ContactsController : BaseApiController
{
    private readonly IContactSettingsService _contactService;
    private readonly ILogger<ContactsController> _logger;

    public ContactsController(IContactSettingsService contactService, ILogger<ContactsController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ContactSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var settings = await _contactService.GetAsync(cancellationToken);
        if (settings == null)
            return NotFound();

        return Ok(settings);
    }

    [HttpPut]
    [ProducesResponseType(typeof(ContactSettingsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] UpdateContactSettingsDto updateDto, CancellationToken cancellationToken)
    {
        var settings = await _contactService.UpdateAsync(updateDto, cancellationToken);
        return Ok(settings);
    }
}