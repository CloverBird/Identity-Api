namespace Guildly.Identity.Application.Queries.GetIdentityUser;

public record GetIdentityUserQueryResult(
    Guid UserId,
    string Email,
    string? Phone,
    DateTime CreatedAt);