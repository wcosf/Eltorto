using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Eltorto.Application.Services;

public sealed class SiteUrlProvider : ISiteUrlProvider
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SiteUrlProvider(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetBaseUrl()
    {
        var configured = _configuration["Smtp:BaseUrl"] ?? _configuration["Site:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Request != null)
        {
            return $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        }

        return string.Empty;
    }
}