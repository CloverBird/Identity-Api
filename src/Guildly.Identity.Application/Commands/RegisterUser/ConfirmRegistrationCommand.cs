using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.RegisterUser;

public record ConfirmRegistrationCommand(
    string Email,
    string Code) : ICommand;