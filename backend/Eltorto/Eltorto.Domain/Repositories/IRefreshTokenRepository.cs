using Eltorto.Domain.Entities;

namespace Eltorto.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default);
}