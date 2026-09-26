using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.RemovePhone;

public record RemovePhoneCommand(
    Guid UserId,
    string Password) : ICommand;