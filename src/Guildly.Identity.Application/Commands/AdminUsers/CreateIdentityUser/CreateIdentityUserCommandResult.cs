using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;

public record CreateIdentityUserCommandResult(
    Guid Id,
    string Email,
    UserStatus Status,
    bool EmailConfirmed,
    DateTimeOffset CreatedAt);
