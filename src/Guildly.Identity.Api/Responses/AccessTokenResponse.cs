namespace Guildly.Identity.Api.Responses;

public record AccessTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);