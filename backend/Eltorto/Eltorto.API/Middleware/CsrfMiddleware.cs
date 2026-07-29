namespace Eltorto.API.Middleware;

public class CsrfMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CsrfMiddleware> _logger;

    public CsrfMiddleware(RequestDelegate next, ILogger<CsrfMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";

        if (IsMutatingMethod(method) && !IsExcludedPath(path))
        {
            var cookieToken = context.Request.Cookies["XSRF-TOKEN"];
            var headerToken = context.Request.Headers["X-CSRF-Token"].FirstOrDefault();

            if (string.IsNullOrEmpty(cookieToken) ||
                string.IsNullOrEmpty(headerToken) ||
                cookieToken != headerToken)
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning(
                    "[SECURITY] CSRF validation failed: IP {IP}, Method {Method}, Path {Path}",
                    ip, method, path);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { error = "CSRF token validation failed" });
                return;
            }
        }

        await _next(context);
    }

    private static bool IsMutatingMethod(string method) => method switch
    {
        "POST" => true,
        "PUT" => true,
        "DELETE" => true,
        "PATCH" => true,
        _ => false
    };

    private static bool IsExcludedPath(string path) =>
        string.Equals(path, "/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(path, "/api/auth/register", StringComparison.OrdinalIgnoreCase);
}
