using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;

namespace Guildly.Identity.Domain.UnitTests.Entities;

public class VerificationSessionTests
{
    private static VerificationSession CreateSession(
        VerificationPurpose purpose = VerificationPurpose.RegisterEmail,
        VerificationChannel channel = VerificationChannel.Email,
        string? sentTo = null,
        string codeHash = "code-hash",
        DateTimeOffset? createdAt = null,
        DateTimeOffset? expiresAt = null)
    {
        var now = DateTimeOffset.UtcNow;

        sentTo ??= channel == VerificationChannel.Email
            ? "user@gmail.com"
            : "+380741739347";

        return VerificationSession.Create(
            Guid.NewGuid(),
            sentTo,
            purpose,
            channel,
            codeHash,
            expiresAt ?? now.AddMinutes(10),
            createdAt ?? now).Value;
    }
    
    [Fact]
    public void Create_Should_CreateVerificationSession_WhenDataIsValid()
    {
        var userId = Guid.NewGuid();
        var sentTo = "user@gmail.com";
        var purpose = VerificationPurpose.RegisterEmail;
        var channel = VerificationChannel.Email;
        var codeHash = "code-hash";
        var createdAt = DateTimeOffset.UtcNow;
        var expiresAt = createdAt.AddMinutes(10);

        var result = VerificationSession.Create(
            userId,
            sentTo,
            purpose,
            channel,
            codeHash,
            expiresAt,
            createdAt);

        Assert.True(result.IsSuccess);

        var session = result.Value;
        Assert.NotEqual(Guid.Empty, session.Id);
        Assert.Equal(userId, session.UserId);
        Assert.Equal(sentTo, session.SentTo);
        Assert.Equal(purpose, session.Purpose);
        Assert.Equal(channel, session.Channel);
        Assert.Equal(codeHash, session.CodeHash);
        Assert.Equal(expiresAt, session.ExpiresAt);
        Assert.Equal(createdAt, session.CreatedAt);
        Assert.Null(session.UsedAt);
        Assert.Null(session.CancelledAt);
        Assert.Equal(0, session.FailedAttempts);
    }
    
    [Theory]
    [InlineData(VerificationPurpose.RegisterEmail, VerificationChannel.Sms)]
    [InlineData(VerificationPurpose.ChangeEmail, VerificationChannel.Sms)]
    [InlineData(VerificationPurpose.ChangePhone, VerificationChannel.Email)]
    public void Create_Should_ReturnFailure_WhenPurposeAndChannelAreNotCompatible(
        VerificationPurpose purpose,
        VerificationChannel channel)
    {
        var result = VerificationSession.Create(
            Guid.NewGuid(),
            "user@gmail.com",
            purpose,
            channel,
            "code-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible, result.Error);
    }
    
    [Theory]
    [InlineData(VerificationChannel.Email, "user@gmail.com")]
    [InlineData(VerificationChannel.Sms, "+380741739347")]
    public void Create_Should_AllowAnyChannel_WhenPurposeIsResetPassword(
        VerificationChannel channel,
        string sentTo)
    {
        var result = VerificationSession.Create(
            Guid.NewGuid(),
            sentTo,
            VerificationPurpose.ResetPassword,
            channel,
            "code-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_Should_ReturnFailure_WhenCodeHashIsEmpty(string? codeHash)
    {
        var result = VerificationSession.Create(
            Guid.NewGuid(),
            "user@gmail.com",
            VerificationPurpose.RegisterEmail,
            VerificationChannel.Email,
            codeHash!,
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.CodeHashIsEmpty, result.Error);
    }
    
    [Fact]
    public void Create_Should_ReturnFailure_WhenEmailChannelSentToIsInvalidEmail()
    {
        var result = VerificationSession.Create(
            Guid.NewGuid(),
            "invalid-email",
            VerificationPurpose.RegisterEmail,
            VerificationChannel.Email,
            "code-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.EmailInvalidFormat, result.Error);
    }
    
    [Fact]
    public void Create_Should_ReturnFailure_WhenSmsChannelSentToIsInvalidPhone()
    {
        var result = VerificationSession.Create(
            Guid.NewGuid(),
            "invalid-phone",
            VerificationPurpose.ChangePhone,
            VerificationChannel.Sms,
            "code-hash",
            DateTimeOffset.UtcNow.AddMinutes(10),
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.PhoneInvalidFormat, result.Error);
    }
    
    [Fact]
    public void EnsureCodeCanBeConfirmed_Should_ReturnSuccess_WhenSessionCanBeConfirmed()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        var result = session.EnsureCodeCanBeConfirmed(now, maxFailedAttempts: 3);

        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public void EnsureCodeCanBeConfirmed_Should_ReturnFailure_WhenSessionIsExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now.AddMinutes(-20),
            expiresAt: now.AddMinutes(-1));

        var result = session.EnsureCodeCanBeConfirmed(now, maxFailedAttempts: 3);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionExpired, result.Error);
    }
    
    [Fact]
    public void EnsureCodeCanBeConfirmed_Should_ReturnFailure_WhenCodeIsAlreadyUsed()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        session.MarkUsed(now);

        var result = session.EnsureCodeCanBeConfirmed(now, maxFailedAttempts: 3);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.CodeIsAlreadyUsed, result.Error);
    }
    
    [Fact]
    public void EnsureCodeCanBeConfirmed_Should_ReturnFailure_WhenSessionIsCancelled()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        session.Cancel(now);

        var result = session.EnsureCodeCanBeConfirmed(now, maxFailedAttempts: 3);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.VerificationSessionIsCancelled, result.Error);
    }
    
    [Fact]
    public void EnsureCodeCanBeConfirmed_Should_ReturnFailure_WhenTooManyFailedAttempts()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        session.RegisterFailedAttempt();
        session.RegisterFailedAttempt();
        session.RegisterFailedAttempt();

        var result = session.EnsureCodeCanBeConfirmed(now, maxFailedAttempts: 3);

        Assert.True(result.IsFailure);
        Assert.Equal(IdentityDomainErrors.TooManyFailedAttempts, result.Error);
    }
    
    [Fact]
    public void MarkUsed_Should_SetUsedAt()
    {
        var session = CreateSession();
        var usedAt = DateTimeOffset.UtcNow;

        session.MarkUsed(usedAt);

        Assert.Equal(usedAt, session.UsedAt);
    }
    
    [Fact]
    public void RegisterFailedAttempt_Should_IncrementFailedAttempts()
    {
        var session = CreateSession();

        session.RegisterFailedAttempt();
        session.RegisterFailedAttempt();

        Assert.Equal(2, session.FailedAttempts);
    }
    
    [Fact]
    public void IsActive_Should_ReturnTrue_WhenSessionIsActive()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        var result = session.IsActive(now);

        Assert.True(result);
    }

    [Fact]
    public void IsActive_Should_ReturnFalse_WhenSessionIsExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now.AddMinutes(-20),
            expiresAt: now.AddMinutes(-1));

        var result = session.IsActive(now);

        Assert.False(result);
    }
    
    [Fact]
    public void IsActive_Should_ReturnFalse_WhenSessionIsUsed()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        session.MarkUsed(now);

        var result = session.IsActive(now);

        Assert.False(result);
    }
    
    [Fact]
    public void IsActive_Should_ReturnFalse_WhenSessionIsCancelled()
    {
        var now = DateTimeOffset.UtcNow;
        var session = CreateSession(
            createdAt: now,
            expiresAt: now.AddMinutes(10));

        session.Cancel(now);

        var result = session.IsActive(now);

        Assert.False(result);
    }
    
    [Fact]
    public void Cancel_Should_SetCancelledAt()
    {
        var session = CreateSession();
        var cancelledAt = DateTimeOffset.UtcNow;

        session.Cancel(cancelledAt);

        Assert.Equal(cancelledAt, session.CancelledAt);
    }
    
    [Fact]
    public void Cancel_Should_NotChangeCancelledAt_WhenSessionIsAlreadyCancelled()
    {
        var session = CreateSession();
        var firstCancelledAt = DateTimeOffset.UtcNow;
        var secondCancelledAt = firstCancelledAt.AddMinutes(1);

        session.Cancel(firstCancelledAt);
        session.Cancel(secondCancelledAt);

        Assert.Equal(firstCancelledAt, session.CancelledAt);
    }
    
    [Fact]
    public void Cancel_Should_NotCancel_WhenSessionIsAlreadyUsed()
    {
        var session = CreateSession();
        var usedAt = DateTimeOffset.UtcNow;

        session.MarkUsed(usedAt);
        session.Cancel(usedAt.AddMinutes(1));

        Assert.Equal(usedAt, session.UsedAt);
        Assert.Null(session.CancelledAt);
    }
}