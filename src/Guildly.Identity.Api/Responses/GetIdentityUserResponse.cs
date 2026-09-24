namespace Guildly.Identity.Api.Responses;

public record GetIdentityUserResponse(
    Guid Id,
    string Email,
    string? Phone,
    DateTime CreatedAt);