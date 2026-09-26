using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.Logout;

public record LogoutCommand(string Token) : ICommand;