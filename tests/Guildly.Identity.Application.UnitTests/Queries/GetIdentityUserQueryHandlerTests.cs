using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Queries.AdminUsers;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Moq;
using static Guildly.Identity.Application.UnitTests.IdentityTestDataHelpers;

namespace Guildly.Identity.Application.UnitTests.Queries;

public class GetIdentityUserQueryHandlerTests
{
    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Moderator)]
    public async Task GetIdentityUser_Should_ReturnNotFound_ForPrivilegedAccounts(UserRole role)
    {
        var user = CreateUser(role: role);
        var repository = new Mock<IIdentityUserRepository>();
        repository.Setup(r => r.GetUserByIdAsync(user.Id, true, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(user);
        var handler = new GetIdentityUserQueryHandler(repository.Object);

        var result = await handler.HandleAsync(new GetIdentityUserQuery(user.Id));

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityApplicationErrors.UserNotFound, result.Error);
    }
}
