using Guildly.Identity.Application.Commands.Logout;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Moq;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class LogoutFromAllDevicesCommandHandlerTests
{
    private readonly LogoutFromAllDevicesCommandHandler _handler;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWork;
    
    public LogoutFromAllDevicesCommandHandlerTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _identityUnitOfWork = new Mock<IIdentityUnitOfWork>();
        _handler = new LogoutFromAllDevicesCommandHandler(
            _refreshTokenServiceMock.Object,
            _identityUnitOfWork.Object);
    }
    
    [Fact]
    public async Task HandleAsync_Should_RevokeAllActiveTokens_AndSaveChanges()
    {
        var command = new LogoutFromAllDevicesCommand(Guid.NewGuid());

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);

        _refreshTokenServiceMock.Verify(
            s => s.RevokeAllActiveTokensAsync(
                command.UserId,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _identityUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}