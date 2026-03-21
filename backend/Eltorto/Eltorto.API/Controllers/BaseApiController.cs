using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

/// <summary>
/// Базовый контроллер для всех API контроллеров
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
}