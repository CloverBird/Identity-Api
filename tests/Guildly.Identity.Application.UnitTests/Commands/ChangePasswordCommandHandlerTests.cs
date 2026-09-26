using Guildly.Identity.Application.Commands.ChangePassword;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();

    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _identityUnitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _handler = new ChangePasswordCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _passwordServiceMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        var command = new ChangePasswordCommand(
            Guid.NewGuid(),
            "new-password",
            "old-password");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserNotFound);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        _passwordServiceMock.Verify(
            s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenOldPasswordIsWrong()
    {
        var passwordHash = "old-password-hash";
        var user = CreateUser(passwordHash: passwordHash);

        var command = new ChangePasswordCommand(
            user.Id,
            "new-password",
            "wrong-old-password");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.OldPassword, user.PasswordHash))
            .Returns(false);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidPassword, result.Error);
        Assert.Equal(passwordHash, user.PasswordHash);

        _passwordServiceMock.Verify(
            s => s.HashPassword(It.IsAny<string>()),
            Times.Never);

        _verificationSessionServiceMock.Verify(
            s => s.CancelActiveVerificationSessionsAsync(
                It.IsAny<Guid>(), It.IsAny<VerificationPurpose>(),
                It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _refreshTokenServiceMock.Verify(
            s => s.RevokeAllActiveTokensAsync(
                It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ChangePassword_CancelResetSessions_RevokeRefreshTokens_AndSaveChanges()
    {
        var user = CreateUser();

        var command = new ChangePasswordCommand(
            user.Id,
            "new-password",
            "old-password");

        var newPasswordHash = "new-password-hash";

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.OldPassword, user.PasswordHash))
            .Returns(true);

        _passwordServiceMock
            .Setup(s => s.HashPassword(command.NewPassword))
            .Returns(newPasswordHash);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPasswordHash, user.PasswordHash);

        _verificationSessionServiceMock.Verify(
            s => s.CancelActiveVerificationSessionsAsync(
                user.Id, VerificationPurpose.ResetPassword,
                It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _refreshTokenServiceMock.Verify(
            s => s.RevokeAllActiveTokensAsync(
                user.Id,
                It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}