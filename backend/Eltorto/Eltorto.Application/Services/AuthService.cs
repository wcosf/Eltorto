using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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

    public AuthService(
       UserManager<AppUser> userManager,
       RoleManager<IdentityRole> roleManager,
       IConfiguration configuration,
       IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Incorrect username or password");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = GenerateJwtToken(user, roles);
        var refreshToken = await GenerateAndStoreRefreshTokenAsync(user);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
        if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        storedToken.IsUsed = true;
        await _unitOfWork.RefreshTokens.UpdateAsync(storedToken);
        await _unitOfWork.SaveChangesAsync();

        var newAccessToken = GenerateJwtToken(user, roles);
        var newRefreshToken = await GenerateAndStoreRefreshTokenAsync(user, storedToken.DeviceInfo);

        return new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };
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
    }
    public async Task<bool> RegisterAsync(RegisterRequest request, string role = "Customer")
    {
        var user = new AppUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new Exception($"Registration error: {string.Join(", ", result.Errors.Select(e => e.Description))}");

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(user, role);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(string userName, ChangePasswordRequest request)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        return result.Succeeded;
    }

    public async Task<bool> CreateAdminIfNotExistsAsync()
    {
        await CreateRoleIfNotExistsAsync("Admin");
        await CreateRoleIfNotExistsAsync("Customer");

        var adminUser = await _userManager.FindByNameAsync("admin");
        if (adminUser != null) return false;

        var registerRequest = new RegisterRequest
        {
            UserName = "admin",
            Email = "admin@eltorto.ru",
            Password = "Admin123!",
            FullName = "Administrator"
        };

        await RegisterAsync(registerRequest, "Admin");
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
            expires: DateTime.UtcNow.AddHours(1),
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