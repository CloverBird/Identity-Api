using Guildly.Common.Results;
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

namespace Guildly.Identity.Application.UnitTests.Commands.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock = new();
    private readonly Mock<IVerificationSessionService> _verificationSessionServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();

    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _identityUserRepositoryMock.Object,
            _verificationSessionServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _passwordServiceMock.Object);
    }

    private void VerifySaveChanges(Times times)
    {
        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            times);
    }

    private void SetupExistsByEmail(bool result = false)
    {
        _identityUserRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }
    
    private readonly RegisterUserCommand _command = new("user@gmail.com", "username", "display name", "password");

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var command = new RegisterUserCommand(
            "invalid-email",
            "username",
            "display name",
            "password");

        _passwordServiceMock
            .Setup(s => s.HashPassword(command.Password))
            .Returns("password-hash");

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.ExistsByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenEmailAlreadyExists()
    {
        _passwordServiceMock
            .Setup(s => s.HashPassword(_command.Password))
            .Returns("password-hash");

        SetupExistsByEmail(true);

        var result = await _handler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserWithEmailAlreadyExists, result.Error);

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
    public async Task HandleAsync_Should_ReturnFailure_WhenStartingVerificationSessionFails()
    {
        _passwordServiceMock
            .Setup(s => s.HashPassword(_command.Password))
            .Returns("password-hash");

        SetupExistsByEmail();

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                It.IsAny<Guid>(),
                _command.Email,
                VerificationPurpose.RegisterEmail,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible);

        var result = await _handler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.Add(It.IsAny<IdentityUser>()),
            Times.Never);

        VerifySaveChanges(Times.Never());
    }

    [Fact]
    public async Task HandleAsync_Should_CreateIdentityUser_CreateUsersProfile_AndSaveChanges_WhenDataIsValid()
    {
        IdentityUser? addedUser = null;

        _passwordServiceMock
            .Setup(s => s.HashPassword(_command.Password))
            .Returns("password-hash");

        SetupExistsByEmail();

        _verificationSessionServiceMock
            .Setup(s => s.StartVerificationSessionAsync(
                It.IsAny<Guid>(),
                _command.Email,
                VerificationPurpose.RegisterEmail,
                VerificationChannel.Email,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StartVerificationSessionResult("123456"));

        _identityUserRepositoryMock
            .Setup(r => r.Add(It.IsAny<IdentityUser>()))
            .Callback<IdentityUser>(user => addedUser = user);

        var result = await _handler.HandleAsync(_command);

        Assert.True(result.IsSuccess);

        Assert.NotNull(addedUser);
        Assert.Equal(_command.Email, addedUser.Email.Value);
        Assert.False(addedUser.EmailConfirmed);
        Assert.Equal(UserStatus.PendingRegistration, addedUser.Status);
        Assert.Equal("password-hash", addedUser.PasswordHash);
        Assert.Equal(UserRole.User, addedUser.Role);

        _identityUserRepositoryMock.Verify(
            r => r.Add(It.IsAny<IdentityUser>()),
            Times.Once);

        VerifySaveChanges(Times.Once());
    }
}