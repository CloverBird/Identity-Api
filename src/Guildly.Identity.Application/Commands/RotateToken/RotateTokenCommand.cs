using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.RotateToken;

public record RotateTokenCommand(string RefreshToken) : ICommand;