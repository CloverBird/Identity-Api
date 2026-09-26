using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.Logout;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Moq;

namespace Guildly.Identity.Application.UnitTests.Commands;

public class LogoutCommandHandlerTests
{
    private readonly LogoutCommandHandler _handler;
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IIdentityUnitOfWork> _identityUnitOfWork;
    
    public LogoutCommandHandlerTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _identityUnitOfWork = new Mock<IIdentityUnitOfWork>();
        _handler = new LogoutCommandHandler(
            _refreshTokenServiceMock.Object,
            _identityUnitOfWork.Object);
    }
    
    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_AndNotSave_WhenRefreshTokenCannotBeRevoked()
    {
        var command = new LogoutCommand("refresh-token");

        _refreshTokenServiceMock
            .Setup(s => s.RevokeTokenAsync(
                command.Token,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityApplicationErrors.InvalidRefreshToken);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.InvalidRefreshToken, result.Error);

        _identityUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_Should_SaveChanges_WhenRefreshTokenIsRevoked()
    {
        var command = new LogoutCommand("refresh-token");

        _refreshTokenServiceMock
            .Setup(s => s.RevokeTokenAsync(
                command.Token,
                It.IsAny<DateTimeOffset>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        var result = await _handler.HandleAsync(command);

        Assert.True(result.IsSuccess);

        _identityUnitOfWork.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}