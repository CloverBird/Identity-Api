namespace Guildly.Identity.Api.Responses;

public record GetIdentityUsersResponse(
    IReadOnlyList<GetIdentityUserResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
