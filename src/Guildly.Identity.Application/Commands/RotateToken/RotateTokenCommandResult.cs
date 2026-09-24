namespace Guildly.Identity.Application.Commands.RotateToken;

public record RotateTokenCommandResult(
    string NewAccessToken, 
    DateTimeOffset NewAccessTokenExpiresAt,
    string NewRefreshToken,
    DateTimeOffset NewRefreshTokenExpiresAt);