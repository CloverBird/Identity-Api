using Guildly.Common.CQRS;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ResetPassword;

public record ConfirmPasswordResetCommand(
    string UserEmail,
    VerificationChannel Channel,
    string NewPassword,
    string Code) : ICommand;