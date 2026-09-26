using Guildly.Identity.Application.Commands.RegisterUser;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
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

public class ResendRegistrationCodeCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();

    private readonly ResendRegistrationCodeCommandHandler _handler;

    public ResendRegistrationCodeCommandHandlerTests()
    {
        _handler = new ResendRegistrationCodeCommandHandler(
            _identityUserRepositoryMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object);
    }

    private void SetupGetUserByEmail(string email, IdentityUser? user)
    {
        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == email),
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void VerifyStartSession(Times times)
    {
        _verificationSessionServiceMock.Verify(
            s => s.StartVerificationSessionAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<VerificationPurpose>(),
                It.IsAny<VerificationChannel>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            times);
    }

    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var command = new ResendRegistrationCodeCommand("invalid-email");

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifyStartSession(Times.Never());
        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        var command = new ResendRegistrationCodeCommand("user@gmail.com");

        SetupGetUserByEmail(command.Email, null);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        VerifyStartSession(Times.Never());
        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserIsBlocked()
    {
        var user = CreateUser(active: false);
        user.Block();

        var command = new ResendRegistrationCodeCommand(user.Email.Value);

        SetupGetUserByEmail(command.Email, user);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserBlocked, result.Error);

        VerifyStartSession(Times.Never());
        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenRegistrationIsAlreadyConfirmed()
    {
        var user = CreateUser();
        var command = new ResendRegistrationCodeCommand(user.Email.Value);

        SetupGetUserByEmail(command.Email, user);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.RegistrationAlreadyConfirmed, result.Error);

        VerifyStartSession(Times.Never());
        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenStartingVerificationSessionFails()
    {
        var user = CreateUser(active: false);
        var command = new ResendRegistrationCodeCommand(user.Email.Value);

        SetupGetUserByEmail(command.Email, user);

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                user.Id,
                user.Email.Value,
                VerificationPurpose.RegisterEmail,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_StartNewRegisterEmailSession_AndSaveChanges_WhenUserHasPendingRegistration()
    {
        var user = CreateUser(active: false);
        var command = new ResendRegistrationCodeCommand(user.Email.Value);

        SetupGetUserByEmail(command.Email, user);

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                user.Id,
                user.Email.Value,
                VerificationPurpose.RegisterEmail,
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
                VerificationPurpose.RegisterEmail,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }
}