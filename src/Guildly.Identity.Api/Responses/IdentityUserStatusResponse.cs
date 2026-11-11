using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Responses;

public record IdentityUserStatusResponse(Guid Id, UserStatus Status);
