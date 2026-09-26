using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.RemovePhone;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class RemovePhoneCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWorkMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();

    private readonly RemovePhoneCommandHandler _removePhoneCommandHandler;

    public RemovePhoneCommandHandlerTests()
    {
        _identityUnitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _removePhoneCommandHandler = new RemovePhoneCommandHandler(
            _identityUserServiceMock.Object,
            _identityUnitOfWorkMock.Object,
            _passwordServiceMock.Object);
        
        _user.ChangePhone("+380741739347");
    }
    
    private readonly IdentityUser _user = CreateUser();
    
    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserNotFound()
    {
        var command = new RemovePhoneCommand(Guid.NewGuid(), "password");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IdentityUser>.Failure(IdentityApplicationErrors.UserNotFound));

        var result = await _removePhoneCommandHandler.HandleAsync(command);

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
    public async Task HandleAsync_Should_ReturnFailure_WhenPasswordIsWrong()
    {
        var command = new RemovePhoneCommand(_user.Id, "wrong-password");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, _user.PasswordHash))
            .Returns(false);

        var result = await _removePhoneCommandHandler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidPassword, result.Error);
        Assert.NotNull(_user.Phone);
        Assert.True(_user.PhoneConfirmed);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_RemovePhoneAndSaveChanges_WhenPasswordIsValid()
    {
        var command = new RemovePhoneCommand(_user.Id, "password");

        _identityUserServiceMock
            .Setup(s => s.GetActiveUserByIdAsync(
                command.UserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_user);

        _passwordServiceMock
            .Setup(s => s.VerifyPassword(command.Password, _user.PasswordHash))
            .Returns(true);

        var result = await _removePhoneCommandHandler.HandleAsync(command);

        Assert.True(result.IsSuccess);
        Assert.Null(_user.Phone);
        Assert.False(_user.PhoneConfirmed);

        _identityUnitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}