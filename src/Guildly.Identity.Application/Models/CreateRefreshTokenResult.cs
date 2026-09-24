namespace Guildly.Identity.Application.Models;

public record CreateRefreshTokenResult(
    string Token,
    DateTimeOffset ExpiresAt);