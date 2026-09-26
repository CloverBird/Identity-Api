using Guildly.Identity.Application.Commands.ResetPassword;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.ResetPassword;

public class StartPasswordResetCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();

    private readonly StartPasswordResetCommandHandler _handler;

    public StartPasswordResetCommandHandlerTests()
    {
        _handler = new StartPasswordResetCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object);
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

    [Fact]
    public async Task HandleAsync_Should_ReturnUserNotFound_WhenUserNotFound()
    {
        var command = new StartPasswordResetCommand(
            "user@gmail.com",
            VerificationChannel.Email);

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByEmailAsync(
                command.UserEmail,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserNotFound);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        _verificationSessionServiceMock.Verify(
            s => s.StartVerificationSessionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<VerificationPurpose>(),
                It.IsAny<VerificationChannel>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenChannelIsNotAccessible()
    {
        var user = CreateUser();
        var command = new StartPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Sms);

        SetupGetActiveUserByEmail(command.UserEmail, user);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.UnsupportedVerificationChannel, result.Error);

        _verificationSessionServiceMock.Verify(
            s => s.StartVerificationSessionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<VerificationPurpose>(),
                It.IsAny<VerificationChannel>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_StartEmailVerificationSession_AndSaveChanges_WhenChannelIsAccessible()
    {
        var user = CreateUser();
        var command = new StartPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Email);

        SetupGetActiveUserByEmail(command.UserEmail, user);

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                user.Id,
                user.Email.Value,
                VerificationPurpose.ResetPassword,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartVerificationSessionResult("123456"));

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);

        _verificationSessionServiceMock.Verify(
            s => s.StartVerificationSessionAsync(
                user.Id,
                user.Email.Value,
                VerificationPurpose.ResetPassword,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenStartingSessionFails()
    {
        var user = CreateUser();
        var command = new StartPasswordResetCommand(
            user.Email.Value,
            VerificationChannel.Email);

        SetupGetActiveUserByEmail(command.UserEmail, user);

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                user.Id,
                user.Email.Value,
                VerificationPurpose.ResetPassword,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible, result.Error);

        VerifySaveChanges(Times.Never());
    }
}