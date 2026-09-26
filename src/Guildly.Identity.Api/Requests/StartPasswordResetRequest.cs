using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Requests;

public record StartPasswordResetRequest(
    string Target,
    VerificationChannel Channel);