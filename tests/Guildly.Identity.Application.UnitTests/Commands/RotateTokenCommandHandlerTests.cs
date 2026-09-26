using Guildly.Identity.Application.Commands.RotateToken;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.Repositories;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class RotateTokenCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock;
    private readonly Mock<IAccessTokenProvider> _accessTokenProviderMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock;
    
    private readonly RotateTokenCommandHandler _rotateTokenCommandHandler;

    public RotateTokenCommandHandlerTests()
    {
        _identityUserRepositoryMock = new();
        _accessTokenProviderMock = new();
        _refreshTokenServiceMock = new();
        _identityUnitOfWorkMock = new();
        
        _rotateTokenCommandHandler = new(
            _identityUserRepositoryMock.Object,
            _accessTokenProviderMock.Object,
            _refreshTokenServiceMock.Object,
            _identityUnitOfWorkMock.Object);
    }

    private void SetupGetRefreshToken(string plainToken, RefreshToken? refreshToken)
    {
        _refreshTokenServiceMock
            .Setup(s => s.GetRefreshTokenAsync(
                plainToken,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
    }
    
    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupGetUserById(IdentityUser? user)
    {
        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                user == null ? It.IsAny<Guid>() : user.Id,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }
    
    private readonly RotateTokenCommand _command = new("refresh-token");
    
    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenRefreshTokenDoesNotExist()
    {
        SetupGetRefreshToken(_command.RefreshToken, null);

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidRefreshToken, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenRefreshTokenIsRevoked()
    {
        var refreshToken = CreateRefreshToken(
            expiresAt: DateTimeOffset.UtcNow.AddDays(1));

        refreshToken.Revoke(DateTimeOffset.UtcNow.AddMinutes(-1));

        SetupGetRefreshToken(_command.RefreshToken, refreshToken);

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenRevoked, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenRefreshTokenIsExpired()
    {
        var refreshToken = CreateRefreshToken(
            createdAt: DateTimeOffset.UtcNow.AddDays(-10),
            expiresAt: DateTimeOffset.UtcNow.AddDays(-1));

        SetupGetRefreshToken(_command.RefreshToken, refreshToken);

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.RefreshTokenExpired, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUserNotFound_WhenUserDoesNotExist()
    {
        var refreshToken = CreateRefreshToken();

        SetupGetRefreshToken(_command.RefreshToken, refreshToken);

        SetupGetUserById(null);

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUserNotActive_WhenUserIsNotActive()
    {
        var user = CreateUser(active: false);
        var refreshToken = CreateRefreshToken(user.Id);

        SetupGetRefreshToken(_command.RefreshToken, refreshToken);

        SetupGetUserById(user);

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_RevokeOldToken_CreateNewTokens_SaveChanges_AndReturnTokens()
    {
        var user = CreateUser();
        var oldRefreshToken = CreateRefreshToken(
            user.Id,
            rememberMe: true,
            expiresAt: DateTimeOffset.UtcNow.AddDays(1));

        var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        SetupGetRefreshToken(_command.RefreshToken, oldRefreshToken);

        SetupGetUserById(user);

        _refreshTokenServiceMock
            .Setup(s => s.CreateRefreshToken(
                user,
                It.IsAny<DateTimeOffset>(),
                oldRefreshToken.RememberMe))
            .Returns(new CreateRefreshTokenResult(
                "new-refresh-token",
                refreshTokenExpiresAt));
        
        _accessTokenProviderMock
            .Setup(p => p.GenerateAccessToken(user))
            .Returns(new AccessToken(
                "new-access-token",
                accessTokenExpiresAt));

        var result = await _rotateTokenCommandHandler.HandleAsync(_command);

        Assert.True(result.IsSuccess);

        Assert.NotNull(oldRefreshToken.RevokedAt);

        Assert.Equal("new-access-token", result.Value.NewAccessToken);
        Assert.Equal(accessTokenExpiresAt, result.Value.NewAccessTokenExpiresAt);
        Assert.Equal("new-refresh-token", result.Value.NewRefreshToken);
        Assert.Equal(refreshTokenExpiresAt, result.Value.NewRefreshTokenExpiresAt);

        _refreshTokenServiceMock.Verify(
            s => s.CreateRefreshToken(
                user,
                It.IsAny<DateTimeOffset>(),
                true),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }
}