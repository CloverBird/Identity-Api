using Guildly.Identity.Application.Commands.AdminUsers.BlockIdentityUser;
using Guildly.Identity.Application.Commands.AdminUsers.UnblockIdentityUser;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Moq;
using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Commands.AdminUsers;

public class IdentityUserBlockingCommandHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _repository = new();
    private readonly Mock<IRefreshTokenService> _refreshTokenService = new();
    private readonly Mock<IIdentityUnitOfWork> _unitOfWork = new();

    [Fact]
    public async Task Block_Should_RevokeRefreshTokensAndSaveBlockedStatus()
    {
        var user = CreateUser();
        SetupUser(user.Id, user);
        _refreshTokenService
            .Setup(service => service.RevokeAllActiveTokensAsync(
                user.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var handler = new BlockIdentityUserCommandHandler(
            _repository.Object, _refreshTokenService.Object, _unitOfWork.Object);

        var result = await handler.HandleAsync(new BlockIdentityUserCommand(user.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Blocked, result.Value);
        Assert.Equal(UserStatus.Blocked, user.Status);
        _refreshTokenService.Verify(service => service.RevokeAllActiveTokensAsync(
            user.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Unblock_Should_SavePendingStatusWhenEmailIsUnconfirmed()
    {
        var user = CreateUser(active: false);
        user.Block();
        SetupUser(user.Id, user);
        var handler = new UnblockIdentityUserCommandHandler(_repository.Object, _unitOfWork.Object);

        var result = await handler.HandleAsync(new UnblockIdentityUserCommand(user.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.PendingRegistration, result.Value);
        Assert.Equal(UserStatus.PendingRegistration, user.Status);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Moderator)]
    public async Task BlockAndUnblock_Should_ReturnNotFoundForMissingOrPrivilegedUser(UserRole? role)
    {
        var userId = Guid.NewGuid();
        var user = role is { } value ? CreateUser(id: userId, role: value) : null;
        SetupUser(userId, user);
        var blockHandler = new BlockIdentityUserCommandHandler(
            _repository.Object, _refreshTokenService.Object, _unitOfWork.Object);
        var unblockHandler = new UnblockIdentityUserCommandHandler(_repository.Object, _unitOfWork.Object);

        var blockResult = await blockHandler.HandleAsync(new BlockIdentityUserCommand(userId));
        var unblockResult = await unblockHandler.HandleAsync(new UnblockIdentityUserCommand(userId));

        Assert.Equal(IdentityApplicationErrors.UserNotFound, blockResult.Error);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, unblockResult.Error);
        _refreshTokenService.VerifyNoOtherCalls();
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private void SetupUser(Guid userId, IdentityUser? user)
    {
        _repository.Setup(repository => repository.GetUserByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(user);
    }
}
