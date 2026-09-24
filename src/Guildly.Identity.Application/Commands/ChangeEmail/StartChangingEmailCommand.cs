using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.ChangeEmail;

public record StartChangingEmailCommand(
    Guid UserId, 
    string Password,
    string NewEmail) : ICommand;