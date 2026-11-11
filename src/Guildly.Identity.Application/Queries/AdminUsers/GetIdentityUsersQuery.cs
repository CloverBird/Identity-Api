using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Queries.AdminUsers;

public record GetIdentityUsersQuery(int Skip, int Take) : IQuery;
