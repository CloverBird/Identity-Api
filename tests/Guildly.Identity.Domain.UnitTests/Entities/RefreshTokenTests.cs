using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.UnitTests.Entities;

public class RefreshTokenTests
{
    private static RefreshToken CreateRefreshToken(
        DateTimeOffset? createdAt = null, DateTimeOffset? expiresAt = null)
    {
        return RefreshToken.Create(
            Guid.NewGuid(),
            "refreshToken",
            true,
            createdAt ?? DateTimeOffset.UtcNow,
            expiresAt ?? DateTimeOffset.UtcNow.AddDays(30));
    }

    [Fact]
    public void Create_Creates_RefreshToken()
    {
        var userId = Guid.NewGuid();
        var tokenHash = "tokenHash";
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = DateTimeOffset.UtcNow.AddDays(30);
        
        var token = RefreshToken.Create(
            userId,
            tokenHash,
            true,
            createdAt,
            expiresAt);
        
        Assert.NotNull(token);
        Assert.Equal(userId, token.UserId);
        Assert.Equal(tokenHash, token.TokenHash);
        Assert.Equal(createdAt, token.CreatedAt);
        Assert.Equal(expiresAt, token.ExpiresAt);
        Assert.True(token.RememberMe);
        Assert.Null(token.RevokedAt);
    }
    
    [Fact]
    public void Revoke_Should_SetRevokedAt()
    {
        var token = CreateRefreshToken();
        var revokedAt = DateTimeOffset.UtcNow;
        
        token.Revoke(revokedAt);

        Assert.Equal(revokedAt, token.RevokedAt);
    }

    [Fact]
    public void EnsureIsActive_Should_ReturnSuccess_WhenTokenIsActive()
    {
        var token = CreateRefreshToken();
        var now = DateTimeOffset.UtcNow;

        var result = token.EnsureIsActive(now);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void EnsureIsActive_Should_ReturnFailure_WhenTokenIsRevoked()
    {
        var token = CreateRefreshToken();
        var now = DateTimeOffset.UtcNow;
        
        token.Revoke(now);

        var result = token.EnsureIsActive(now);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenRevoked, result.Error);
    }

    [Fact]
    public void EnsureIsActive_Should_ReturnFailure_WhenTokenIsExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var refreshToken = CreateRefreshToken(
            now.AddDays(-2),
            now.AddDays(-1));

        var result = refreshToken.EnsureIsActive(now);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenExpired, result.Error);
    }
}