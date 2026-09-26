namespace Guildly.Identity.Api.Requests;

public record ConfirmRegistrationRequest(
    string Email,
    string Code);