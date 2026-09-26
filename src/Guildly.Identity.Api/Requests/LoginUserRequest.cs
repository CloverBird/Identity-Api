namespace Guildly.Identity.Api.Requests;

public record LoginUserRequest(
    string Email,
    string Password,
    bool RememberMe);