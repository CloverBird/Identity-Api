using Guildly.Common.CQRS;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ResetPassword;

public record StartPasswordResetCommand(
    string UserEmail,
    VerificationChannel Channel) : ICommand;