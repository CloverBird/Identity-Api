using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.ChangePhone;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.ChangePhone;

public class ConfirmChangingPhoneCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IIdentityContactsService> _identityContactsServiceMock = new();

    private readonly ConfirmChangingPhoneCommandHandler _handler;

    public ConfirmChangingPhoneCommandHandlerTests()
    {
        _handler = new ConfirmChangingPhoneCommandHandler(
            _identityUserServiceMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _identityContactsServiceMock.Object);
    }

    private static VerificationSession CreateChangePhoneSession(Guid userId, string sentTo = "+380741739347")
    {
        return CreateVerificationSession(
            userId, sentTo,
            VerificationPurpose.ChangePhone, VerificationChannel.Sms);
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

    private void SetupSuccessConfirmSession(IdentityUser user, string code, VerificationSession session)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ChangePhone,
                VerificationChannel.Sms,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    private void SetupFailedConfirmSession(IdentityUser user, string code, Error error)
    {
        _verificationSessionServiceMock
            .Setup(s => s.ConfirmSessionAsync(
                user.Id,
                code,
                It.IsAny<DateTimeOffset>(),
                VerificationPurpose.ChangePhone,
                VerificationChannel.Sms,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(error);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        var command = new ConfirmChangingPhoneCommand(Guid.NewGuid(), "123456");

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
        var command = new ConfirmChangingPhoneCommand(user.Id, "wrong-code");

        SetupGetActiveUserById(user);

        SetupFailedConfirmSession(user, command.Code, IdentityApplicationErrors.InvalidConfirmationCode);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidConfirmationCode, result.Error);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnError_AndNotSave_WhenSessionExpired()
    {
        var user = CreateUser();
        var command = new ConfirmChangingPhoneCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupFailedConfirmSession(user, command.Code, IdentityDomainErrors.VerificationSessionExpired);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_SaveUsedSession_AndReturnError_WhenPhoneIsNoLongerAvailable()
    {
        var user = CreateUser();
        var phone = "+380741739347";
        var session = CreateChangePhoneSession(user.Id, phone);

        var command = new ConfirmChangingPhoneCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupSuccessConfirmSession(user, command.Code, session);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailablePhoneAsync(
                phone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.UserWithPhoneAlreadyExists);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserWithPhoneAlreadyExists, result.Error);
        Assert.Null(user.Phone);

        VerifySaveChanges(Times.Once());
    }

    [Fact]
    public async Task HandleAsync_Should_ChangePhone_AndSaveChanges_WhenCodeIsValid()
    {
        var user = CreateUser();
        var phone = "+380741739347";
        var session = CreateChangePhoneSession(user.Id, phone);

        var command = new ConfirmChangingPhoneCommand(user.Id, "123456");

        SetupGetActiveUserById(user);

        SetupSuccessConfirmSession(user, command.Code, session);

        _identityContactsServiceMock
            .Setup(s => s.GetAvailablePhoneAsync(
                phone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Phone.Create(phone).Value);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(user.Phone);
        Assert.Equal(phone, user.Phone.Value);
        Assert.True(user.PhoneConfirmed);

        VerifySaveChanges(Times.Once());
    }
}