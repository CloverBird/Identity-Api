using Guildly.Common.Domain;
using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<List<RefreshToken>> GetActiveTokensAsync(
        Guid userId, 
        DateTimeOffset now,
        bool asNoTracking = false, 
        CancellationToken cancellationToken = default);
    
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash, 
        bool asNoTracking = false, 
        CancellationToken cancellationToken = default);
}