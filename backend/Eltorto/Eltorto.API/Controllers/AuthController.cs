using System.Security.Claims;
using System.Security.Cryptography;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Eltorto.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var (response, refreshToken) = await _authService.LoginAsync(request);

            SetRefreshTokenCookie(refreshToken, response.Expiration);
            SetCsrfTokenCookie();

            return Ok(new
            {
                accessToken = response.AccessToken,
                expiration = response.Expiration,
                userName = response.UserName,
                roles = response.Roles
            });
        }
        catch (UnauthorizedAccessException)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogWarning("Failed login attempt for user {UserName} from IP {IP}", request.UserName, ip);

            return Unauthorized(new { error = "Invalid credentials" });
        }
    }

    /// <summary>
    /// Registers a new user (customer) with role "Customer".
    /// </summary>
    [HttpPost("register")]
    [EnableRateLimiting("RegisterPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var (succeeded, errors) = await _authService.RegisterAsync(request, "Customer");
            if (!succeeded)
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning("Registration failed for user {UserName} (Email: {Email}) from IP {IP}: {Errors}",
                    request.UserName, request.Email, ip, string.Join("; ", errors));

                return BadRequest(new { error = "Registration failed. Please check your input." });
            }

            return StatusCode(StatusCodes.Status201Created, new { message = "Customer registered successfully" });
        }
        catch (Exception ex)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _logger.LogError(ex, "Unexpected error during registration for user {UserName} from IP {IP}",
                request.UserName, ip);

            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error." });
        }
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token from cookie.
    /// </summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("RefreshPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { error = "No refresh token" });

        try
        {
            var (response, newRefreshToken) = await _authService.RefreshTokenAsync(refreshToken);

            SetRefreshTokenCookie(newRefreshToken, response.Expiration);
            SetCsrfTokenCookie();

            return Ok(new
            {
                accessToken = response.AccessToken,
                expiration = response.Expiration,
                userName = response.UserName,
                roles = response.Roles
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            if (ex.Message.Contains("reuse"))
                _logger.LogWarning("Possible token theft detected from IP {IP}. All user sessions have been revoked.", ip);
            else
                _logger.LogWarning("Invalid refresh token attempt from IP {IP}", ip);

            return Unauthorized(new { error = "Invalid or expired refresh token" });
        }
    }

    /// <summary>
    /// Logs out the user by revoking the refresh token cookie.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken);
        }

        ClearAuthCookies();
        return NoContent();
    }

    /*/// <summary>
    /// Changes the password for the currently authenticated user.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Unauthorized();

        var (succeeded, errors) = await _authService.ChangePasswordAsync(userName, request);
        if (!succeeded)
            return BadRequest(new { error = string.Join("; ", errors) });

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Changes the username for the currently authenticated user.
    /// </summary>
    [HttpPost("change-username")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeUserName([FromBody] ChangeUserNameRequest request)
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Unauthorized();

        try
        {
            var (response, newRefreshToken) = await _authService.ChangeUserNameAsync(userName, request);

            SetRefreshTokenCookie(newRefreshToken, response.Expiration);
            SetCsrfTokenCookie();

            return Ok(new
            {
                accessToken = response.AccessToken,
                expiration = response.Expiration,
                userName = response.UserName,
                roles = response.Roles
            });
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("ChangeUserName failed: user not found (requested by {User})", userName);
            return NotFound(new { error = "User not found." });
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("ChangeUserName failed: incorrect password for user {User}", userName);
            return Unauthorized(new { error = "Invalid password." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("ChangeUserName failed for user {User}: {Error}", userName, ex.Message);
            return Conflict(new { error = "Could not change username." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during ChangeUserName for user {User}", userName);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Internal server error." });
        }
    }*/

    /// <summary>
    /// Returns the current user's info based on the access token.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userName = User.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return Unauthorized();

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();

        return Ok(new
        {
            userName,
            roles
        });
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiration)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expiration.AddDays(7)
        };
        Response.Cookies.Append("refresh_token", refreshToken, cookieOptions);
    }

    private void SetCsrfTokenCookie()
    {
        var csrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("XSRF-TOKEN", csrfToken, cookieOptions);
    }

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/" });
        Response.Cookies.Delete("XSRF-TOKEN", new CookieOptions { Path = "/" });
    }
}
