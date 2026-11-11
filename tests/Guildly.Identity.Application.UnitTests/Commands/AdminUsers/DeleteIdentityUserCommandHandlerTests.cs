using Guildly.Identity.Application.Commands.AdminUsers.DeleteIdentityUser;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Moq;
using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.AdminUsers;

public class DeleteIdentityUserCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _repository = new();
    private readonly Mock<IIdentityUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task HandleAsync_Should_DeleteOrdinaryUserAndSaveChanges()
    {
        var user = CreateUser();
        _repository.Setup(repository => repository.GetUserByIdAsync(user.Id, false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(user);
        var handler = new DeleteIdentityUserCommandHandler(_repository.Object, _unitOfWork.Object);

        var result = await handler.HandleAsync(new DeleteIdentityUserCommand(user.Id));

        Assert.True(result.IsSuccess);
        _repository.Verify(repository => repository.Remove(user), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Moderator)]
    public async Task HandleAsync_Should_ReturnNotFound_WhenUserIsMissingOrPrivileged(UserRole? role)
    {
        var userId = Guid.NewGuid();
        var user = role is { } value ? CreateUser(id: userId, role: value) : null;
        _repository.Setup(repository => repository.GetUserByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(user);
        var handler = new DeleteIdentityUserCommandHandler(_repository.Object, _unitOfWork.Object);

        var result = await handler.HandleAsync(new DeleteIdentityUserCommand(userId));

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);
        _repository.Verify(repository => repository.Remove(It.IsAny<IdentityUser>()), Times.Never);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
