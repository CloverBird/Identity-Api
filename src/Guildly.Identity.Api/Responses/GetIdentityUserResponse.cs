using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Responses;

public record GetIdentityUserResponse(
    Guid Id,
    string Email,
    UserStatus Status,
    bool EmailConfirmed,
    DateTimeOffset CreatedAt);
