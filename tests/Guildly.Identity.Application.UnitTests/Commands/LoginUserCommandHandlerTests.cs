using Guildly.Identity.Application.Commands.Login;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock;
    private readonly Mock<IAccessTokenProvider> _accessTokenProviderMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    
    private readonly LoginUserCommandHandler _loginUserCommandHandler;

    public LoginUserCommandHandlerTests()
    {
        _identityUserRepositoryMock = new();
        _refreshTokenServiceMock = new();
        _identityUnitOfWorkMock = new();
        _accessTokenProviderMock = new();
        _passwordServiceMock = new();
        
        _loginUserCommandHandler = new(
            _identityUserRepositoryMock.Object, 
            _passwordServiceMock.Object, 
            _accessTokenProviderMock.Object, 
            _refreshTokenServiceMock.Object, 
            _identityUnitOfWorkMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var command = new LoginUserCommand("invalid-email", "password", false);

        var result = await _loginUserCommandHandler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidCredentials, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        var command = new LoginUserCommand("user@gmail.com", "password", false);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _loginUserCommandHandler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidCredentials, result.Error);

        _passwordServiceMock.Verify(
            s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenPasswordIsInvalid()
    {
        var user = CreateUser();
        var command = new LoginUserCommand(user.Email.Value, "wrong-password", false);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        var result = await _loginUserCommandHandler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidCredentials, result.Error);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserIsNotActive()
    {
        var user = IdentityUser.Create(
            Guid.NewGuid(),
            "user@gmail.com",
            "password-hash",
            UserRole.User,
            DateTimeOffset.UtcNow).Value;

        var command = new LoginUserCommand(user.Email.Value, "password", false);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        var result = await _loginUserCommandHandler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);

        _accessTokenProviderMock.Verify(
            p => p.GenerateAccessToken(It.IsAny<IdentityUser>()),
            Times.Never);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnTokens_AndSaveChanges_WhenCredentialsAreValid()
    {
        var user = CreateUser();
        var command = new LoginUserCommand(user.Email.Value, "password", true);

        var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _accessTokenProviderMock
            .Setup(p => p.GenerateAccessToken(user))
            .Returns(new AccessToken("access-token", accessTokenExpiresAt));

        _refreshTokenServiceMock
            .Setup(s => s.CreateRefreshToken(
                user,
                It.IsAny<DateTimeOffset>(),
                command.RememberMe))
            .Returns(new CreateRefreshTokenResult("refresh-token", refreshTokenExpiresAt));

        var result = await _loginUserCommandHandler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal(accessTokenExpiresAt, result.Value.AccessTokenExpiresAt);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal(refreshTokenExpiresAt, result.Value.RefreshTokenExpiresAt);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}