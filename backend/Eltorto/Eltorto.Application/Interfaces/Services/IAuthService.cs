using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IAuthService
{
    Task<(LoginResponse Response, string RefreshToken)> LoginAsync(LoginRequest request);
    Task<(bool Succeeded, string[] Errors)> RegisterAsync(RegisterRequest request, string role = "Admin");
    Task<(bool Succeeded, string[] Errors)> ChangePasswordAsync(string userName, ChangePasswordRequest request);
    Task<(LoginResponse Response, string RefreshToken)> ChangeUserNameAsync(string userName, ChangeUserNameRequest request);
    Task<bool> CreateAdminIfNotExistsAsync();
    Task<bool> CreateRoleIfNotExistsAsync(string roleName);

    Task<(LoginResponse Response, string RefreshToken)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserTokensAsync(string userId);
}