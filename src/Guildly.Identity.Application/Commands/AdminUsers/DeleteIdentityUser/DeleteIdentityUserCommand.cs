using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.AdminUsers.DeleteIdentityUser;

public record DeleteIdentityUserCommand(Guid UserId) : ICommand;
