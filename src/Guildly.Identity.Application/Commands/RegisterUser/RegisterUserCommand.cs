using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Username,
    string? DisplayName,
    string Password) : ICommand;