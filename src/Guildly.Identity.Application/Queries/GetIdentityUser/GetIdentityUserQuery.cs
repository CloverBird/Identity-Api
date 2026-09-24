using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Queries.GetIdentityUser;

public record GetIdentityUserQuery(Guid UserId) : IQuery;