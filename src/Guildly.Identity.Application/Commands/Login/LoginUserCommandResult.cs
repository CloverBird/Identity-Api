namespace Guildly.Identity.Application.Commands.Login;

public record LoginUserCommandResult(
    string AccessToken, 
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);