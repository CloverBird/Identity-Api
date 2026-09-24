using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.RegisterUser;

public record ResendRegistrationCodeCommand(string Email) : ICommand;