namespace Guildly.Identity.Api.Requests;

public record StartChangingEmailRequest(
    string NewEmail,
    string Password);