namespace Guildly.Identity.Api.Requests;

public record ChangePasswordRequest(
    string OldPassword,
    string NewPassword);