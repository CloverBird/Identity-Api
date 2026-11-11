using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

namespace Guildly.Identity.Application.UnitTests.Commands.AdminUsers;

public class CreateIdentityUserCommandHandlerTests
{
    private readonly Mock<IIdentityContactsService> _contactsService = new();
    private readonly Mock<IPasswordService> _passwordService = new();
    private readonly Mock<IIdentityUserRepository> _repository = new();
    private readonly Mock<IIdentityUnitOfWork> _unitOfWork = new();
    private readonly CreateIdentityUserCommand _command = new("user@example.com", "test-password");
    private readonly CreateIdentityUserCommandHandler _handler;

    public CreateIdentityUserCommandHandlerTests()
    {
        _handler = new CreateIdentityUserCommandHandler(
            _contactsService.Object, _passwordService.Object, _repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_CreatePendingUserWithHashedPassword()
    {
        SetupAvailableEmail();
        IdentityUser? addedUser = null;
        _repository.Setup(repository => repository.Add(It.IsAny<IdentityUser>()))
                   .Callback<IdentityUser>(user => addedUser = user);
        _unitOfWork.Setup(unitOfWork => unitOfWork.TrySaveIdentityUserChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Success);

        var result = await _handler.HandleAsync(_command);

        Assert.True(result.IsSuccess);
        Assert.NotNull(addedUser);
        Assert.Equal(addedUser.Id, result.Value.Id);
        Assert.Equal(_command.Email, result.Value.Email);
        Assert.Equal(UserRole.User, addedUser.Role);
        Assert.Equal(UserStatus.PendingRegistration, result.Value.Status);
        Assert.False(result.Value.EmailConfirmed);
        Assert.Equal("hashed-password", addedUser.PasswordHash);
    }

    [Fact]
    public async Task HandleAsync_Should_RejectDuplicateEmailBeforeAddingUser()
    {
        _contactsService.Setup(service => service.GetAvailableEmailAsync(_command.Email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Result<Email>.Failure(IdentityApplicationErrors.UserWithEmailAlreadyExists));

        var result = await _handler.HandleAsync(_command);

        Assert.Equal(IdentityApplicationErrors.UserWithEmailAlreadyExists, result.Error);
        _repository.Verify(repository => repository.Add(It.IsAny<IdentityUser>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.TrySaveIdentityUserChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnConflict_WhenEmailIsTakenDuringSave()
    {
        SetupAvailableEmail();
        _unitOfWork.Setup(unitOfWork => unitOfWork.TrySaveIdentityUserChangesAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(Result.Failure(IdentityApplicationErrors.UserWithEmailAlreadyExists));

        var result = await _handler.HandleAsync(_command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserWithEmailAlreadyExists, result.Error);
    }

    private void SetupAvailableEmail()
    {
        _contactsService.Setup(service => service.GetAvailableEmailAsync(_command.Email, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Email.Create(_command.Email));
        _passwordService.Setup(service => service.HashPassword(_command.Password)).Returns("hashed-password");
    }
}
