using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Eltorto.API.Extensions;

public static class JwtExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKey)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var path = context.HttpContext.Request.Path;

                    logger.LogWarning(
                        "[SECURITY] JWT authentication failed: IP {IP}, Path {Path}, Exception {Exception}",
                        ip, path, context.Exception?.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var path = context.HttpContext.Request.Path;

                    logger.LogWarning(
                        "[SECURITY] JWT challenge (401): IP {IP}, Path {Path}, Error {Error}",
                        ip, path, context.ErrorDescription);
                    return Task.CompletedTask;
                },
                OnForbidden = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<JwtBearerEvents>>();
                    var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var path = context.HttpContext.Request.Path;
                    var user = context.HttpContext.User.Identity?.Name ?? "anonymous";

                    logger.LogWarning(
                        "[SECURITY] JWT forbidden (403): User {User}, IP {IP}, Path {Path}",
                        user, ip, path);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}