namespace Guildly.Identity.Api.Responses;

public record GetMeAsUserResponse(
    Guid Id,
    string Email,
    string? Phone,
    DateTime CreatedAt);
