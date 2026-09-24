using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.Logout;

public record LogoutFromAllDevicesCommand(Guid UserId) : ICommand;