using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.ChangePhone;

public record StartChangingPhoneCommand(
    Guid UserId,
    string Password,
    string Phone) : ICommand;