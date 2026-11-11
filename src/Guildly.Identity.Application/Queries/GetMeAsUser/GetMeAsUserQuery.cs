using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Queries.GetMeAsUser;

public record GetMeAsUserQuery(Guid UserId) : IQuery;
