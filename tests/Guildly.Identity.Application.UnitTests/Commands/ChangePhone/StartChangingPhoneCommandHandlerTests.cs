using Guildly.Identity.Application.Commands.ChangePhone;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.ChangePhone;

public class StartChangingPhoneCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IIdentityContactsService> _identityContactsServiceMock = new();

    private readonly StartChangingPhoneCommandHandler _handler;

    public StartChangingPhoneCommandHandlerTests()
    {
        _handler = new StartChangingPhoneCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _passwordServiceMock.Object,
            _identityContactsServiceMock.Object);
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

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        var command = new StartChangingPhoneCommand(
            Guid.NewGuid(),
            "password",
            "+380741739347");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserNotFound);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        _passwordServiceMock.Verify(
            s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnInvalidPassword_WhenPasswordIsWrong()
    {
        var user = CreateUser();

        var command = new StartChangingPhoneCommand(
            user.Id,
            "wrong-password",
            "+380741739347");

        SetupGetActiveUserById(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidPassword, result.Error);

        _identityContactsServiceMock.Verify(
            s => s.GetAvailablePhoneAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenPhoneIsNotAvailable()
    {
        var user = CreateUser();

        var command = new StartChangingPhoneCommand(
            user.Id,
            "password",
            "+380741739347");

        SetupGetActiveUserById(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailablePhoneAsync(
                command.Phone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserWithPhoneAlreadyExists);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserWithPhoneAlreadyExists, result.Error);

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
    public async Task HandleAsync_Should_StartVerificationSession_AndSaveChanges_WhenRequestIsValid()
    {
        var user = CreateUser();
        var phone = "+380741739347";

        var command = new StartChangingPhoneCommand(
            user.Id,
            "password",
            phone);

        SetupGetActiveUserById(user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailablePhoneAsync(
                command.Phone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Phone.Create(phone).Value);

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                user.Id,
                phone,
                VerificationPurpose.ChangePhone,
                VerificationChannel.Sms,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartVerificationSessionResult("123456"));

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);

        _verificationSessionServiceMock.Verify(
            s => s.StartVerificationSessionAsync(
                user.Id,
                phone,
                VerificationPurpose.ChangePhone,
                VerificationChannel.Sms,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }
}