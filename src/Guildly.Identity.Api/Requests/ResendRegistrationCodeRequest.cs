using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Requests;

public record ResendRegistrationCodeRequest(string Email);