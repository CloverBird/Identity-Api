using Guildly.Common.Results;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Errors;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Domain.Entities;

public class IdentityUser
{
    public Guid Id { get; private set; }
    
    public Email Email { get; private set; }
    public bool EmailConfirmed { get; private set; }
    
    public Phone? Phone { get; private set; } 
    public bool PhoneConfirmed { get; private set; }
    
    public string PasswordHash { get; private set; } = string.Empty;
    
    public UserStatus Status { get; private set; }
    
    public UserRole Role {get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    
    private IdentityUser() { }

    private IdentityUser(
        Guid id, 
        Email email,
        bool emailConfirmed,
        Phone? phone,
        bool phoneConfirmed,
        string passwordHash,
        UserStatus status,
        UserRole role,
        DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        EmailConfirmed = emailConfirmed;
        Phone = phone;
        PhoneConfirmed = phoneConfirmed;
        PasswordHash = passwordHash;
        Status = status;
        Role = role;
        CreatedAt = createdAt;
    }

    public static Result<IdentityUser> Create(
        Guid id, 
        string email, 
        string passwordHash,
        UserRole role,
        DateTimeOffset createdAt)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }
        
        return new IdentityUser(
            id, 
            emailResult.Value, 
            false, 
            null, 
            false, 
            passwordHash,
            UserStatus.PendingRegistration,
            role,
            createdAt);
    }

    public Result ConfirmEmail()
    {
        if (Status == UserStatus.Blocked)
        {
            return IdentityDomainErrors.UserBlocked;
        }

        EmailConfirmed = true;
        Status = UserStatus.Active;
        
        return Result.Success;
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
    }

    public Result ConfirmPhone()
    {
        if (Status == UserStatus.Blocked)
        {
            return IdentityDomainErrors.UserBlocked;
        }

        if (Phone == null)
        {
            return IdentityDomainErrors.PhoneNotProvided;
        }
        
        PhoneConfirmed = true;
        return Result.Success;
    }

    public Result ChangeEmail(string email)
    {
        if (Status == UserStatus.Blocked)
        {
            return IdentityDomainErrors.UserBlocked;
        }
        
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            // if ChangeEmail is called, means that email was already confirmed with code that was successfully delivered to it
            // so in general this situation should not be reachable 
            throw new InvalidOperationException($"Verification session for user {Id} contains invalid pending email.");
        }
        
        Email = emailResult.Value;
        EmailConfirmed = true;
        
        return Result.Success;
    }

    public Result<string> GetContact(VerificationChannel channel)
    {
        if (Status != UserStatus.Active)
        {
            return IdentityDomainErrors.UserIsNotActive;
        }

        return channel switch
        {
            VerificationChannel.Email when EmailConfirmed => Email.Value,
            VerificationChannel.Sms when Phone != null && PhoneConfirmed => Phone.Value,
            _ => IdentityDomainErrors.UnsupportedVerificationChannel
        };
    }

    public Result ChangePhone(string phone)
    {
        if (Status == UserStatus.Blocked)
        {
            return IdentityDomainErrors.UserBlocked;
        }
        
        var phoneResult = Phone.Create(phone);
        if (phoneResult.IsFailure)
        {
            // if ChangePhone is called, means that phone number was already confirmed with code that was successfully delivered to it
            // so in general this situation should not be reachable 
            throw new InvalidOperationException($"Verification session for user {Id} contains invalid phone number.");
        }
        
        Phone = phoneResult.Value;
        PhoneConfirmed = true;
        
        return Result.Success;
    }

    public Result RemovePhone()
    {
        if (Status == UserStatus.Blocked)
        {
            return IdentityDomainErrors.UserBlocked;
        }
        
        Phone = null;
        PhoneConfirmed = false;
        
        return Result.Success;
    }
    
    public Result ChangePassword(string passwordHash)
    {
        if (Status is not UserStatus.Active)
        {
            return IdentityDomainErrors.UserIsNotActive;
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return IdentityDomainErrors.PasswordIsEmpty;
        }
        
        PasswordHash = passwordHash;
        
        return Result.Success;
    }
}