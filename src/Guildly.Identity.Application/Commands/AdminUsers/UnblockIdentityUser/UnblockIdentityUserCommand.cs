using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.AdminUsers.UnblockIdentityUser;

public record UnblockIdentityUserCommand(Guid UserId) : ICommand;
