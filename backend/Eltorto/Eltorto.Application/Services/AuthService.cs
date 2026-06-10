using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eltorto.Application.DTOs;
using Eltorto.Application.Interfaces.Services;
using Eltorto.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Eltorto.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Incorrect username or password");

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        return new LoginResponse
        {
            Token = token,
            Expiration = DateTime.UtcNow.AddHours(1),
            UserName = user.UserName!,
            Roles = roles.ToArray()
        };
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
}