using Eltorto.Application.DTOs;

namespace Eltorto.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request, string role = "Admin");
    Task<bool> ChangePasswordAsync(string userName, ChangePasswordRequest request);
    Task<bool> CreateAdminIfNotExistsAsync();
    Task<bool> CreateRoleIfNotExistsAsync(string roleName);

    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task RevokeAllUserTokensAsync(string userId);
}