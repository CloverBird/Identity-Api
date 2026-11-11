namespace Guildly.Identity.Application.Queries.AdminUsers;

public record GetIdentityUsersQueryResult(
    IReadOnlyList<GetIdentityUserQueryResult> Items,
    int TotalCount);
