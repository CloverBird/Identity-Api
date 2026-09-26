using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.Repositories;
using Microsoft.Extensions.Options;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Services;

public class RefreshTokenServiceTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IRefreshTokenProvider> _refreshTokenProviderMock;
    private readonly RefreshTokenService _refreshTokenService;

    private readonly RefreshTokenConfiguration _configuration = new()
    {
        RefreshTokenLifeTime = 5,
        RememberMeRefreshTokenLifeTime = 30,
        Length = 32,
        SecretKey = "secret"
    };

    public RefreshTokenServiceTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _refreshTokenProviderMock = new Mock<IRefreshTokenProvider>();

        _refreshTokenService = new RefreshTokenService(
            _refreshTokenRepositoryMock.Object,
            _refreshTokenProviderMock.Object,
            Options.Create(_configuration));
    }
    
    private readonly string _plainToken = "plain-token";
    private readonly string _tokenHash = "token-hash";
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void CreateRefreshToken_Should_AddRefreshToken_AndReturnPlainToken()
    {
        var user = CreateUser();
        var createdAt = DateTimeOffset.UtcNow;

        RefreshToken? addedRefreshToken = null;

        _refreshTokenProviderMock
            .Setup(p => p.GenerateRefreshToken(user))
            .Returns(_plainToken);

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.Add(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => addedRefreshToken = token);

        var result = _refreshTokenService.CreateRefreshToken(
            user,
            createdAt,
            rememberMe: false);

        Assert.Equal(_plainToken, result.Token);
        Assert.Equal(createdAt.AddDays(_configuration.RefreshTokenLifeTime), result.ExpiresAt);

        Assert.NotNull(addedRefreshToken);
        Assert.Equal(user.Id, addedRefreshToken.UserId);
        Assert.Equal(_tokenHash, addedRefreshToken.TokenHash);
        Assert.False(addedRefreshToken.RememberMe);
        Assert.Equal(createdAt, addedRefreshToken.CreatedAt);
        Assert.Equal(createdAt.AddDays(_configuration.RefreshTokenLifeTime), addedRefreshToken.ExpiresAt);
        Assert.Null(addedRefreshToken.RevokedAt);

        _refreshTokenRepositoryMock.Verify(
            r => r.Add(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public void CreateRefreshToken_Should_UseRememberMeLifetime_WhenRememberMeIsTrue()
    {
        var user = CreateUser();

        RefreshToken? addedRefreshToken = null;

        _refreshTokenProviderMock
            .Setup(p => p.GenerateRefreshToken(user))
            .Returns(_plainToken);

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.Add(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => addedRefreshToken = token);

        var result = _refreshTokenService.CreateRefreshToken(
            user,
            _now,
            rememberMe: true);

        Assert.Equal(_plainToken, result.Token);
        Assert.Equal(_now.AddDays(_configuration.RememberMeRefreshTokenLifeTime), result.ExpiresAt);

        Assert.NotNull(addedRefreshToken);
        Assert.True(addedRefreshToken.RememberMe);
        Assert.Equal(_now.AddDays(_configuration.RememberMeRefreshTokenLifeTime), addedRefreshToken.ExpiresAt);
    }

    [Fact]
    public async Task GetRefreshToken_Should_HashToken_AndLoadByHash()
    {
        var refreshToken = CreateRefreshToken(tokenHash: _tokenHash);

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(
                _tokenHash,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result = await _refreshTokenService.GetRefreshTokenAsync(
            _plainToken,
            asNoTracking: true);

        Assert.Same(refreshToken, result);

        _refreshTokenRepositoryMock.Verify(
            r => r.GetByTokenHashAsync(
                _tokenHash,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RevokeToken_Should_ReturnError_WhenTokenDoesNotExist()
    {
        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(
                _tokenHash,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var result = await _refreshTokenService.RevokeTokenAsync(
            _plainToken,
            _now);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task RevokeToken_Should_ReturnFailure_WhenTokenIsExpired()
    {
        var refreshToken = CreateRefreshToken(
            tokenHash: _tokenHash,
            createdAt: _now.AddDays(-10),
            expiresAt: _now.AddDays(-1));

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(
                _tokenHash,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result = await _refreshTokenService.RevokeTokenAsync(
            _plainToken,
            _now);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenExpired, result.Error);
        Assert.Null(refreshToken.RevokedAt);
    }

    [Fact]
    public async Task RevokeToken_Should_ReturnFailure_WhenTokenIsAlreadyRevoked()
    {
        var refreshToken = CreateRefreshToken(
            tokenHash: _tokenHash,
            createdAt: _now,
            expiresAt: _now.AddDays(5));

        refreshToken.Revoke(_now.AddMinutes(-1));

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(
                _tokenHash,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result = await _refreshTokenService.RevokeTokenAsync(
            _plainToken,
            _now);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenRevoked, result.Error);
    }

    [Fact]
    public async Task RevokeToken_Should_RevokeToken_WhenTokenIsActive()
    {
        var refreshToken = CreateRefreshToken(
            tokenHash: _tokenHash,
            createdAt: _now,
            expiresAt: _now.AddDays(5));

        _refreshTokenProviderMock
            .Setup(p => p.HashRefreshToken(_plainToken))
            .Returns(_tokenHash);

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenHashAsync(
                _tokenHash,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        var result = await _refreshTokenService.RevokeTokenAsync(
            _plainToken,
            _now);

        Assert.True(result.IsSuccess);
        Assert.Equal(_now, refreshToken.RevokedAt);
    }

    [Fact]
    public async Task RevokeAllActiveTokens_Should_RevokeAllReturnedTokens()
    {
        var userId = Guid.NewGuid();

        var firstToken = CreateRefreshToken(
            userId: userId,
            expiresAt: _now.AddDays(1));

        var secondToken = CreateRefreshToken(
            userId: userId,
            expiresAt: _now.AddDays(2));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetActiveTokensAsync(
                userId,
                _now,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([firstToken, secondToken]);

        await _refreshTokenService.RevokeAllActiveTokensAsync(
            userId,
            _now);

        Assert.Equal(_now, firstToken.RevokedAt);
        Assert.Equal(_now, secondToken.RevokedAt);

        _refreshTokenRepositoryMock.Verify(
            r => r.GetActiveTokensAsync(
                userId,
                _now,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}