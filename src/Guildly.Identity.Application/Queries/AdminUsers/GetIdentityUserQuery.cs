using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Queries.AdminUsers;

public record GetIdentityUserQuery(Guid UserId) : IQuery;
