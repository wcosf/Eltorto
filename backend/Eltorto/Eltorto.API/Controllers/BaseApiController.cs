using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Eltorto.API.Controllers;

/// <summary>
/// Base controller for all API controllers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("GlobalPolicy")]
public abstract class BaseApiController : ControllerBase
{
}