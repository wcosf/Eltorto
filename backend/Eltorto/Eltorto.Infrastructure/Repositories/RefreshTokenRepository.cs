using Eltorto.Application.Interfaces.Repositories;
using Eltorto.Domain.Entities;
using Eltorto.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Eltorto.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
    }
}