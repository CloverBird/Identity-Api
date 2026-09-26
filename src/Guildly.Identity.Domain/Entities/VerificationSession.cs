using Guildly.Common.Results;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Domain.Entities;

public class VerificationSession
{
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    
    public string SentTo { get; private set; } = string.Empty; 
    
    public VerificationPurpose Purpose { get; private set; }

    public VerificationChannel Channel { get; private set; }
    
    public string CodeHash { get; private set; } = string.Empty;
    
    public DateTimeOffset ExpiresAt { get; private set; }
    
    public DateTimeOffset? UsedAt { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    
    public DateTimeOffset? CancelledAt { get; private set; }

    public int FailedAttempts  { get; private set; }
    
    private VerificationSession() {}

    private VerificationSession(
        Guid id,
        Guid userId,
        string sentTo,
        VerificationPurpose purpose,
        VerificationChannel channel,
        string codeHash,
        DateTimeOffset expiresAt,
        DateTimeOffset? usedAt,
        DateTimeOffset createdAt,
        DateTimeOffset? cancelledAt,
        int failedAttempts)
    {
        Id = id;
        UserId = userId;
        SentTo = sentTo;
        Purpose = purpose;
        Channel = channel;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
        UsedAt = usedAt;
        CreatedAt = createdAt;
        CancelledAt = cancelledAt;
        FailedAttempts = failedAttempts;
    }
    
    public static Result<VerificationSession> Create(
        Guid userId,
        string sentTo,
        VerificationPurpose purpose,
        VerificationChannel channel,
        string codeHash,
        DateTimeOffset expiresAt,
        DateTimeOffset createdAt)
    {
        if (purpose is not VerificationPurpose.ResetPassword)
        {
            if ((channel is VerificationChannel.Email &&
                purpose is not (VerificationPurpose.RegisterEmail or VerificationPurpose.ChangeEmail)) ||
                (channel is VerificationChannel.Sms &&
                purpose is not VerificationPurpose.ChangePhone))
            {
                return IdentityDomainErrors.VerificationPurposeAndChannelAreNotCompatible;
            }
        }
        
        if (string.IsNullOrWhiteSpace(codeHash))
        {
            return IdentityDomainErrors.CodeHashIsEmpty;
        }

        var error = channel switch
        {
            VerificationChannel.Email => Email.Create(sentTo).Error,
            VerificationChannel.Sms => Phone.Create(sentTo).Error,
            _ => Error.None
        };

        if (error.ErrorType is not ErrorType.None)
        {
            return error;
        }
        
        return new VerificationSession(
            Guid.NewGuid(),
            userId,
            sentTo,
            purpose,
            channel,
            codeHash,
            expiresAt,
            null,
            createdAt,
            null,
            0);
    }

    public Result EnsureCodeCanBeConfirmed(DateTimeOffset now, int maxFailedAttempts)
    {
        if (ExpiresAt <= now)
        {
            return IdentityDomainErrors.VerificationSessionExpired;
        }
        
        if (UsedAt is not null)
        {
            return IdentityDomainErrors.CodeIsAlreadyUsed;
        }

        if (CancelledAt is not null)
        {
            return IdentityDomainErrors.VerificationSessionIsCancelled;
        }

        if (FailedAttempts >= maxFailedAttempts)
        {
            return IdentityDomainErrors.TooManyFailedAttempts;
        }
        
        return Result.Success;
    }    

    public void MarkUsed(DateTimeOffset usedAt)
    {
        UsedAt = usedAt;
    }
    
    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
    }

    public bool IsActive(DateTimeOffset now)
    {
        return CancelledAt == null &&
               UsedAt == null &&
               ExpiresAt > now;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        if (UsedAt is not null || CancelledAt is not null)
        {
            return;
        }

        CancelledAt = cancelledAt;
    }
}