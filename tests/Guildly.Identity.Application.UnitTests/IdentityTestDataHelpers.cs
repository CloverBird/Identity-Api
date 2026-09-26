using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.UnitTests;

public static class IdentityTestDataHelpers
{
    public static IdentityUser CreateUser(
        Guid? id = null,
        string email = "user@gmail.com",
        string passwordHash = "password-hash",
        UserRole role = UserRole.User,
        bool active = true)
    {
        var user = IdentityUser.Create(
            id ?? Guid.NewGuid(),
            email,
            passwordHash,
            role,
            DateTimeOffset.UtcNow).Value;

        if (active)
        {
            user.ConfirmEmail();
        }

        return user;
    }

    public static RefreshToken CreateRefreshToken(
        Guid? userId = null,
        string tokenHash = "refresh-token-hash",
        bool rememberMe = false,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? expiresAt = null)
    {
        var now = DateTimeOffset.UtcNow;

        return RefreshToken.Create(
            userId ?? Guid.NewGuid(),
            tokenHash,
            rememberMe,
            createdAt ?? now,
            expiresAt ?? now.AddDays(1));
    }

    public static VerificationSession CreateVerificationSession(
        Guid userId,
        string sentTo,
        VerificationPurpose purpose,
        VerificationChannel channel,
        string codeHash = "code-hash",
        DateTimeOffset? expiresAt = null,
        DateTimeOffset? createdAt = null)
    {
        var now = DateTimeOffset.UtcNow;

        return VerificationSession.Create(
            userId,
            sentTo,
            purpose,
            channel,
            codeHash,
            expiresAt ?? now.AddMinutes(15),
            createdAt ?? now).Value;
    }
}