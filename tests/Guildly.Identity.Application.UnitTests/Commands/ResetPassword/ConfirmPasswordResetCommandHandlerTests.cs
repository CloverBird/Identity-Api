using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.ResetPassword;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.ResetPassword;

public class ConfirmPasswordResetCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();

    private readonly ConfirmPasswordResetCommandHandler _handler;

    public ConfirmPasswordResetCommandHandlerTests()
    {
        _handler = new ConfirmPasswordResetCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _refreshTokenServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _passwordServiceMock.Object);
    }

    private static VerificationSession CreateResetPasswordSession(Guid userId, VerificationChannel channel, string sentTo)
    {
        return CreateVerificationSession(
            userId, sentTo,
            VerificationPurpose.ResetPassword, channel);
    }

    private void SetupGetActiveUserByEmail(string email, IdentityUser user)
    {
        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByEmailAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupConfirmSessionSuccess(
        IdentityUser user, string code, VerificationChannel channel, VerificationSession session)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ResetPassword,
                channel,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    private void SetupConfirmSessionFailure(
        IdentityUser user, string code, VerificationChannel channel, Error error)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ResetPassword,
                channel,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUserNotFound_WhenUserNotFound()
    {
        var command = new ConfirmPasswordResetCommand(
            "user@gmail.com",
            VerificationChannel.Email,
            "new-password",
            "123456");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByEmailAsync(
                command.UserEmail,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserNotFound);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        _verificationSessionServiceMock.Verify(
            s => s.ConfirmSessionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<VerificationPurpose?>(),
                It.IsAny<VerificationChannel?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_SaveFailedAttempt_AndReturnError_WhenCodeIsInvalid()
    {
        var user = CreateUser();
        var command = new ConfirmPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Email,
            "new-password",
            "wrong-code");

        SetupGetActiveUserByEmail(command.UserEmail, user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            command.Channel,
            IdentityApplicationErrors.InvalidConfirmationCode);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidConfirmationCode, result.Error);

        _passwordServiceMock.Verify(
            s => s.HashPassword(It.IsAny<string>()),
            Times.Never);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnError_AndNotSave_WhenSessionExpired()
    {
        var user = CreateUser();
        var command = new ConfirmPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Email,
            "new-password",
            "123456");

        SetupGetActiveUserByEmail(command.UserEmail, user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            command.Channel,
            IdentityDomainErrors.VerificationSessionExpired);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);

        _passwordServiceMock.Verify(
            s => s.HashPassword(It.IsAny<string>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ChangePassword_RevokeRefreshTokens_AndSaveChanges_WhenCodeIsValid()
    {
        var user = CreateUser(passwordHash: "old-password-hash");
        var session = CreateResetPasswordSession(
            user.Id,
            VerificationChannel.Email,
            user.Email.Value);

        var command = new ConfirmPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Email,
            "new-password",
            "123456");

        var newPasswordHash = "new-password-hash";

        SetupGetActiveUserByEmail(command.UserEmail, user);

        SetupConfirmSessionSuccess(
            user,
            command.Code,
            command.Channel,
            session);

        _passwordServiceMock
            .Setup(s => s.HashPassword(command.NewPassword))
            .Returns(newPasswordHash);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(newPasswordHash, user.PasswordHash);

        _refreshTokenServiceMock.Verify(
            s => s.RevokeAllActiveTokensAsync(
                user.Id,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }
}