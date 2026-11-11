using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.AdminUsers.BlockIdentityUser;

public record BlockIdentityUserCommand(Guid UserId) : ICommand;
