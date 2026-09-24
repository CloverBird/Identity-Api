using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.ChangeEmail;

public record ConfirmChangingEmailCommand(
    Guid UserId,
    string Code) : ICommand;