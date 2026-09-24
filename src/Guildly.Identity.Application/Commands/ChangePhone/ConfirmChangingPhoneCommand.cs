using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.ChangePhone;

public record ConfirmChangingPhoneCommand(
    Guid UserId,
    string Code) : ICommand;