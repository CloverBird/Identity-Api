using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.ChangeEmail;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.ChangeEmail;

public class ConfirmChangingEmailCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IIdentityContactsService> _identityContactsServiceMock = new();

    private readonly ConfirmChangingEmailCommandHandler _handler;

    public ConfirmChangingEmailCommandHandlerTests()
    {
        _handler = new ConfirmChangingEmailCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _identityContactsServiceMock.Object);
    }

    private static VerificationSession CreateChangeEmailSession(Guid userId, string sentTo = "new@gmail.com")
    {
        return CreateVerificationSession(
            userId,
            sentTo,
            VerificationPurpose.ChangeEmail,
            VerificationChannel.Email);
    }

    private void SetupGetActiveUserById(IdentityUser user)
    {
        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupConfirmSessionSuccess(IdentityUser user, string code, VerificationSession session)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ChangeEmail,
                VerificationChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    private void SetupConfirmSessionFailure(IdentityUser user, string code, Error error)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ChangeEmail,
                VerificationChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        var command = new ConfirmChangingEmailCommand(Guid.NewGuid(), "123456");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
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
        var command = new ConfirmChangingEmailCommand(user.Id, "wrong-code");

        SetupGetActiveUserById(user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            IdentityApplicationErrors.InvalidConfirmationCode);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidConfirmationCode, result.Error);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnError_AndNotSave_WhenSessionExpired()
    {
        var user = CreateUser();
        var command = new ConfirmChangingEmailCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            IdentityDomainErrors.VerificationSessionExpired);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_SaveUsedSession_AndReturnError_WhenEmailIsNoAvailable()
    {
        var user = CreateUser();
        var oldEmail = user.Email;
        var email = "new@gmail.com";
        var session = CreateChangeEmailSession(user.Id, email);

        var command = new ConfirmChangingEmailCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupConfirmSessionSuccess(user, command.Code, session);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailableEmailAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserWithEmailAlreadyExists);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserWithEmailAlreadyExists, result.Error);
        Assert.Equal(oldEmail, user.Email);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ChangeEmail_AndSaveChanges_WhenCodeIsValid()
    {
        var user = CreateUser();
        var email = "new@gmail.com";
        var session = CreateChangeEmailSession(user.Id, email);

        var command = new ConfirmChangingEmailCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupConfirmSessionSuccess(user, command.Code, session);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailableEmailAsync(
                email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Email.Create(email).Value);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Equal(email, user.Email.Value);
        Assert.True(user.EmailConfirmed);

        VerifySaveChanges(Times.Once());
    }
}