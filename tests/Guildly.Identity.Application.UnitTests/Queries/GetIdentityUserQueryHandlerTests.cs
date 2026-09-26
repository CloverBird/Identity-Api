using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Queries.GetIdentityUser;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Repositories;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Queries;

public class GetIdentityUserQueryHandlerTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock = new();

    private readonly GetIdentityUserQueryHandler _handler;

    public GetIdentityUserQueryHandlerTests()
    {
        _handler = new GetIdentityUserQueryHandler(_identityUserRepositoryMock.Object);
    }
    
    [Fact]
    public async Task HandleAsync_Should_ReturnFailure_WhenUserDoesNotExist()
    {
        var query = new GetIdentityUserQuery(Guid.NewGuid());

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                query.UserId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByIdAsync(
                query.UserId,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_ReturnUser_WhenUserExists()
    {
        var user = CreateUser();
        user.ChangePhone("+380553731383");
        var query = new GetIdentityUserQuery(user.Id);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                query.UserId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.HandleAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal(user.Email.Value, result.Value.Email);
        Assert.Equal(user.Phone!.Value, result.Value.Phone);
        Assert.Equal(user.CreatedAt.DateTime, result.Value.CreatedAt);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByIdAsync(
                query.UserId,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}