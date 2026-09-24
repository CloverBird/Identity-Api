using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Requests;

public record ConfirmPasswordResetRequest(
    string Target,
    VerificationChannel Channel,
    string NewPassword,
    string Code);