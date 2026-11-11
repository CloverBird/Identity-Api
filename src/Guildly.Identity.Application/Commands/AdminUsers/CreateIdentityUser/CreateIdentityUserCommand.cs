using Guildly.Common.CQRS;

namespace Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;

public record CreateIdentityUserCommand(string Email, string Password) : ICommand;
