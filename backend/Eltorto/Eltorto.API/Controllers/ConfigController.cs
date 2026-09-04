using Microsoft.AspNetCore.Mvc;

namespace Eltorto.API.Controllers;

public class ConfigController : BaseApiController
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("client")]
    public IActionResult GetClientConfig()
    {
        var siteKey = _configuration["Recaptcha:SiteKey"] ?? string.Empty;
        return Ok(new { recaptchaSiteKey = siteKey });
    }
}
