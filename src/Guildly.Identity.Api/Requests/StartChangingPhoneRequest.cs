namespace Guildly.Identity.Api.Requests;

public record StartChangingPhoneRequest(
    string NewPhone,
    string Password);