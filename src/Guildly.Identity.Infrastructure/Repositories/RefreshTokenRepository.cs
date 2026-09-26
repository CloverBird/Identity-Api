using Guildly.Common.Database.Repository;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Identity.Infrastructure.Repositories;

internal class RefreshTokenRepository : Repository<RefreshToken, IdentityDbContext>, IRefreshTokenRepository
{
    public RefreshTokenRepository(IdentityDbContext context) 
        : base(context)
    {
    }

    public Task<List<RefreshToken>> GetActiveTokensAsync(
        Guid userId, 
        DateTimeOffset now,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.RefreshTokens.AsNoTracking() : DbContext.RefreshTokens;
        
        return query.Where(
                r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > now)
                    .ToListAsync(cancellationToken);
    }

    public Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash, 
        bool asNoTracking = false, 
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.RefreshTokens.AsNoTracking() : DbContext.RefreshTokens;
        
        return query.FirstOrDefaultAsync(
            r => r.TokenHash == tokenHash,
            cancellationToken);
    }
}