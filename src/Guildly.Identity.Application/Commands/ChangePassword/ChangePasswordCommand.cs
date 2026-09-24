using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string NewPassword,
    string OldPassword) : ICommand;