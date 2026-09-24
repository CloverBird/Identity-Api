using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Queries.GetIdentityUser;

internal class GetIdentityUserQueryHandler : IQueryHandler<GetIdentityUserQuery, GetIdentityUserQueryResult>
{
    // TODO: do i need query reader?
    private readonly IIdentityUserRepository _identityUserRepository;

    public GetIdentityUserQueryHandler(IIdentityUserRepository identityUserRepository)
    {
        _identityUserRepository = identityUserRepository;
    }

    public async Task<Result<GetIdentityUserQueryResult>> HandleAsync(GetIdentityUserQuery query, CancellationToken cancellationToken = default)
    {
        var user = await _identityUserRepository.GetUserByIdAsync(query.UserId, true, cancellationToken);
        if (user is null)
        {
            return IdentityApplicationErrors.UserNotFound;
        }
        
        return new GetIdentityUserQueryResult(user.Id, user.Email.Value, user.Phone?.Value, user.CreatedAt.DateTime);
    }
}