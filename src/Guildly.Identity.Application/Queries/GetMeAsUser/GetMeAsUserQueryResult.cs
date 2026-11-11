namespace Guildly.Identity.Application.Queries.GetMeAsUser;

public record GetMeAsUserQueryResult(
    Guid UserId,
    string Email,
    string? Phone,
    DateTime CreatedAt);
