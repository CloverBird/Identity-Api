using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Queries.AdminUsers;

internal class GetIdentityUsersQueryHandler(IIdentityUserRepository identityUserRepository)
    : IQueryHandler<GetIdentityUsersQuery, GetIdentityUsersQueryResult>
{
    public async Task<Result<GetIdentityUsersQueryResult>> HandleAsync(
        GetIdentityUsersQuery query, CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await identityUserRepository.GetIdentityUsersAsync(
            UserRole.User, query.Skip, query.Take, cancellationToken);

        var items = users.Select(user => new GetIdentityUserQueryResult(
            user.Id, user.Email.Value, user.Status, user.EmailConfirmed, user.CreatedAt)).ToArray();

        return new GetIdentityUsersQueryResult(items, totalCount);
    }
}
