using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;
using Moq;

using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Services;

public class IdentityUserServiceTests
{
    private readonly Mock<IIdentityUserRepository> _identityUserRepositoryMock;
    private readonly IdentityUserService _identityUserService;

    public IdentityUserServiceTests()
    {
        _identityUserRepositoryMock = new Mock<IIdentityUserRepository>();
        _identityUserService = new IdentityUserService(_identityUserRepositoryMock.Object);
    }

    [Fact]
    public async Task GetActiveUserByIdAsync_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                userId,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _identityUserService.GetActiveUserByIdAsync(userId);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByIdAsync_Should_ReturnUserNotActive_WhenUserIsPendingRegistration()
    {
        var user = CreateUser(active: false);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                user.Id,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByIdAsync(user.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByIdAsync_Should_ReturnUserNotActive_WhenUserIsBlocked()
    {
        var user = CreateUser();
        user.Block();

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                user.Id,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByIdAsync(user.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByIdAsync_Should_ReturnUser_WhenUserIsActive()
    {
        var user = CreateUser();

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByIdAsync(
                user.Id,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByIdAsync(user.Id);

        Assert.True(result.IsSuccess);
        Assert.Same(user, result.Value);
    }

    [Fact]
    public async Task GetActiveUserByEmailAsync_Should_ReturnFailure_WhenEmailIsInvalid()
    {
        var result = await _identityUserService.GetActiveUserByEmailAsync("invalid-email");

        Assert.True(result.IsFailure);

        _identityUserRepositoryMock.Verify(
            r => r.GetUserByEmailAsync(
                It.IsAny<Email>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetActiveUserByEmailAsync_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        var email = "user@gmail.com";

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == email),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IdentityUser?)null);

        var result = await _identityUserService.GetActiveUserByEmailAsync(email);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByEmailAsync_Should_ReturnUserNotActive_WhenUserIsPendingRegistration()
    {
        var user = CreateUser(active: false);

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == user.Email.Value),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByEmailAsync(user.Email.Value);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByEmailAsync_Should_ReturnUserNotActive_WhenUserIsBlocked()
    {
        var user = CreateUser();
        user.Block();

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == user.Email.Value),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByEmailAsync(user.Email.Value);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotActive, result.Error);
    }

    [Fact]
    public async Task GetActiveUserByEmailAsync_Should_ReturnUser_WhenUserIsActive()
    {
        var user = CreateUser();

        _identityUserRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(
                It.Is<Email>(e => e.Value == user.Email.Value),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _identityUserService.GetActiveUserByEmailAsync(user.Email.Value);

        Assert.True(result.IsSuccess);
        Assert.Same(user, result.Value);
    }
}