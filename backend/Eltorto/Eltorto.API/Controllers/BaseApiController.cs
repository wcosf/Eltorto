using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

/// <summary>
/// Base controller for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}