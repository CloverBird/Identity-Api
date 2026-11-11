using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Queries.AdminUsers;

internal class GetIdentityUserQueryHandler(IIdentityUserRepository identityUserRepository)
    : IQueryHandler<GetIdentityUserQuery, GetIdentityUserQueryResult>
{
    public async Task<Result<GetIdentityUserQueryResult>> HandleAsync(
        GetIdentityUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await identityUserRepository.GetUserByIdAsync(query.UserId, true, cancellationToken);
        if (user is null || user.Role != UserRole.User)
        {
            return IdentityApplicationErrors.UserNotFound;
        }

        return new GetIdentityUserQueryResult(user.Id, user.Email.Value, user.Status, user.EmailConfirmed, user.CreatedAt);
    }
}
