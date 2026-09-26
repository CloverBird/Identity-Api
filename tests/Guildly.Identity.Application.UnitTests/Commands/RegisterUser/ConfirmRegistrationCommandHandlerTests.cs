using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.RegisterUser;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.RegisterUser;

public class ConfirmRegistrationCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();

    private readonly ConfirmRegistrationCommandHandler _handler;

    public ConfirmRegistrationCommandHandlerTests()
    {
        _handler = new ConfirmRegistrationCommandHandler(
            _identityUserRepositoryMock.Object,
            _identityUnitOfWorkMock.Object,
            _verificationSessionServiceMock.Object);
    }

    private static IdentityUser CreatePendingUser(string email = "user@gmail.com")
    {
        return CreateUser(email: email, active: false);
    }

    private static VerificationSession CreateRegisterEmailSession(Guid userId, string sentTo = "user@gmail.com")
    {
        return CreateVerificationSession(
            userId, sentTo,
            VerificationPurpose.RegisterEmail, VerificationChannel.Email);
    }

    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupGetUserByEmail(string email, IdentityUser? user)
    {
        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == email),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupConfirmSessionSuccess(IdentityUser user, string code, VerificationSession session)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.RegisterEmail,
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
                VerificationPurpose.RegisterEmail,
                VerificationChannel.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var command = new ConfirmRegistrationCommand("invalid-email", "123456");

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        var command = new ConfirmRegistrationCommand("user@gmail.com", "123456");

        SetupGetUserByEmail(command.Email, null);

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
    public async Task HandleAsync_Should_ReturnFailure_WhenUserEmailIsAlreadyConfirmed()
    {
        var user = CreateUser();
        var command = new ConfirmRegistrationCommand(user.Email.Value, "123456");

        SetupGetUserByEmail(command.Email, user);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.RegistrationAlreadyConfirmed, result.Error);

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
    public async Task HandleAsync_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreatePendingUser();
        user.Block();

        var command = new ConfirmRegistrationCommand(user.Email.Value, "123456");

        SetupGetUserByEmail(command.Email, user);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserBlocked, result.Error);

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
        var user = CreatePendingUser();
        var command = new ConfirmRegistrationCommand(user.Email.Value, "wrong-code");

        SetupGetUserByEmail(command.Email, user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            IdentityApplicationErrors.InvalidConfirmationCode);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidConfirmationCode, result.Error);

        Assert.False(user.EmailConfirmed);
        Assert.Equal(UserStatus.PendingRegistration, user.Status);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_Returnfailure_AndNotSave_WhenSessionExpired()
    {
        var user = CreatePendingUser();
        var command = new ConfirmRegistrationCommand(user.Email.Value, "123456");

        SetupGetUserByEmail(command.Email, user);

        SetupConfirmSessionFailure(
            user,
            command.Code,
            IdentityDomainErrors.VerificationSessionExpired);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);

        Assert.False(user.EmailConfirmed);
        Assert.Equal(UserStatus.PendingRegistration, user.Status);

        VerifySaveChanges(Times.Never());
    }

   [Fact]
    public async Task HandleAsync_Should_ConfirmEmail_ActivateUser_AndSaveChanges_WhenCodeIsValid()
    {
        var user = CreatePendingUser();
        var session = CreateRegisterEmailSession(user.Id, user.Email.Value);

        var command = new ConfirmRegistrationCommand(user.Email.Value, "123456");

        SetupGetUserByEmail(command.Email, user);
        SetupConfirmSessionSuccess(user, command.Code, session);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailConfirmed);
        Assert.Equal(UserStatus.Active, user.Status);

        VerifySaveChanges(Times.Once());
    }
}