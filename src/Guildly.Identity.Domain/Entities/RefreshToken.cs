using Guildly.Common.Results;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public string TokenHash { get; private set; } = string.Empty;
    
    public bool RememberMe { get; set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? RevokedAt { get; private set; }
    
    private RefreshToken() { }
    
    private RefreshToken(
        Guid id,
        Guid userId,
        string tokenHash,
        bool rememberMe,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        RememberMe = rememberMe;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
    }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        bool rememberMe,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        return new(
            Guid.NewGuid(),
            userId,
            tokenHash,
            rememberMe,
            createdAt,
            expiresAt,
            null);
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt = revokedAt;
    }

    public Result EnsureIsActive(DateTimeOffset now)
    {
        if (RevokedAt is not null)
        {
            return IdentityDomainErrors.RefreshTokenRevoked;
        }

        if (ExpiresAt <= now)
        {
            return IdentityDomainErrors.RefreshTokenExpired;
        }

        return Result.Success;
    }
}