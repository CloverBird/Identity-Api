namespace Guildly.Identity.Application.Models;

public record AccessToken(
    string Token,
    DateTimeOffset ExpiresAt);