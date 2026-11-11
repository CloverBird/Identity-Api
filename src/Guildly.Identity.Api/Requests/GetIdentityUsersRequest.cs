namespace Guildly.Identity.Api.Requests;

public record GetIdentityUsersRequest
{
    public int? Page { get; init; }

    public int? PageSize { get; init; }
}
