using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Queries.AdminUsers;

public record GetIdentityUserQueryResult(
    Guid Id,
    string Email,
    UserStatus Status,
    bool EmailConfirmed,
    DateTimeOffset CreatedAt);
