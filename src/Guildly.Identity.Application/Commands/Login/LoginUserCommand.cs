using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.Login;

public record LoginUserCommand(
    string Email,
    string Password,
    bool RememberMe) : ICommand;