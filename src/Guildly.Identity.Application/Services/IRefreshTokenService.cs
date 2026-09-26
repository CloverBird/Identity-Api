using Guildly.Common.Results;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Application.Services;

public interface IRefreshTokenService
{
    CreateRefreshTokenResult CreateRefreshToken(
        IdentityUser user, 
        DateTimeOffset createdAt,
        bool rememberMe);
    
    Task<RefreshToken?> GetRefreshTokenAsync(
        string token,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);
    
    Task<Result> RevokeTokenAsync(
        string refreshToken,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default);

    Task RevokeAllActiveTokensAsync(
        Guid userId,
        DateTimeOffset revokedAt,
        CancellationToken cancellationToken = default);
}