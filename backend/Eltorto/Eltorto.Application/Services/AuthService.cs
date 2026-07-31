using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Eltorto.Domain.Abstractions;

namespace Eltorto.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
       UserManager<AppUser> userManager,
       RoleManager<IdentityRole> roleManager,
       IConfiguration configuration,
       IUnitOfWork unitOfWork,
       ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(LoginResponse Response, string RefreshToken)> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Incorrect username or password");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            Expiration = DateTime.UtcNow.AddMinutes(15),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };

        return (response, refreshToken.Token);
    }

    public async Task<(LoginResponse Response, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        if (storedToken.IsRevoked || storedToken.IsUsed)
        {
            var status = storedToken.IsRevoked ? "revoked" : "already used";
            _logger.LogWarning(
                "[AUTH] Possible refresh token theft detected for user {UserId}. Token {TokenId} was {Status}. Revoking all sessions.",
                storedToken.UserId, storedToken.Id, status);

            await RevokeAllUserTokensAsync(storedToken.UserId);
            throw new UnauthorizedAccessException("Refresh token reuse detected");
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        storedToken.IsUsed = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(storedToken);
        await _unitOfWork.SaveChangesAsync();

        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(user, storedToken.DeviceInfo);

        var response = new LoginResponse
        {
            AccessToken = newAccessToken,
            Expiration = DateTime.UtcNow.AddMinutes(15),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };

        return (response, newRefreshToken.Token);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (storedToken == null) return false;

        storedToken.IsRevoked = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(storedToken);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task RevokeAllUserTokensAsync(string userId)
    {
        await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogWarning("[AUTH] All sessions revoked for user {UserId}", userId);
    }
    public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(RegisterRequest request, string role = "Customer")
    {
        var user = new AppUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return (false, result.Errors.Select(e => e.Description).ToArray());

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(user, role);
        return (true, []);
    }

    public async Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userName, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            _logger.LogWarning("[AUTH] ChangePassword failed: user {UserName} not found", userName);
            return (false, ["Пользователь не найден"]);
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            _logger.LogWarning("[AUTH] ChangePassword failed for user {UserName}: {Errors}",
                userName, string.Join("; ", result.Errors.Select(e => e.Description)));
            return (false, result.Errors.Select(e => e.Description).ToArray());
        }

        _logger.LogInformation("[AUTH] Password changed for user {UserName}", userName);
        return (true, []);
    }

    public async Task<(LoginResponse Response, string RefreshToken)> ChangeUserNameAsync(string userName, ChangeUserNameRequest request)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            _logger.LogWarning("[AUTH] ChangeUserName failed: user {UserName} not found", userName);
            throw new KeyNotFoundException("Пользователь не найден");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            _logger.LogWarning("[AUTH] ChangeUserName failed for user {UserName}: incorrect password", userName);
            throw new UnauthorizedAccessException("Неверный пароль");
        }

        var existingUser = await _userManager.FindByNameAsync(request.NewUserName);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            _logger.LogWarning("[AUTH] ChangeUserName failed for user {UserName}: new username {NewUserName} already taken",
                userName, request.NewUserName);
            throw new InvalidOperationException("Логин уже занят");
        }

        var result = await _userManager.SetUserNameAsync(user, request.NewUserName);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("[AUTH] ChangeUserName failed for user {UserName}: {Errors}", userName, errors);
            throw new InvalidOperationException(errors);
        }

        await _userManager.UpdateNormalizedUserNameAsync(user);

        _logger.LogInformation("[AUTH] User {OldUserName} changed username to {NewUserName}", userName, request.NewUserName);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            Expiration = DateTime.UtcNow.AddMinutes(15),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };

        return (response, refreshToken.Token);
    }

    public async Task<bool> CreateAdminIfNotExistsAsync()
    {
        await CreateRoleIfNotExistsAsync("Admin");
        await CreateRoleIfNotExistsAsync("Customer");

        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count != 0) return false;

        var registerRequest = new RegisterRequest
        {
            UserName = "admin",
            Email = "admin@eltorto.ru",
            Password = _configuration["AdminSettings:Password"] ?? "Admin123!",
            FullName = "Administrator"
        };

        var (succeeded, errors) = await RegisterAsync(registerRequest, "Admin");
        if (!succeeded)
            throw new InvalidOperationException($"Failed to create admin: {string.Join("; ", errors)}");
        return true;
    }

    public async Task<bool> CreateRoleIfNotExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            return result.Succeeded;
        }
        return true;
    }

    private string GenerateJwtToken(AppUser user, IList<string> roles)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateAndStoreRefreshTokenAsync(AppUser user, string? deviceInfo = null)
    {
        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false,
            DeviceInfo = deviceInfo ?? "Unknown"
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();
        return refreshToken;
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}