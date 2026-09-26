namespace Guildly.Identity.Api.Requests;

public record RegisterUserRequest(
    string Email,
    string Password,
    string Username,
    string? DisplayName);