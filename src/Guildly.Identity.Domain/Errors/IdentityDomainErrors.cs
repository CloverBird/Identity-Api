using Guildly.Common.Results;
using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Domain.Errors;

public static class IdentityDomainErrors
{
    public static readonly Error EmailRequired = Error.Validation("identity.email.required", "Email is required");
    public static readonly Error EmailTooLong = Error.Validation("identity.email.too_long", "Email is too long");
    public static readonly Error EmailInvalidFormat = Error.Validation("identity.email.invalid", "Email is invalid");
    
    public static readonly Error PhoneRequired = Error.Validation("identity.phone.required", "Phone is required");
    public static readonly Error PhoneTooShort = Error.Validation("identity.phone.too_short", "Phone is too short");
    public static readonly Error PhoneTooLong = Error.Validation("identity.phone.too_long", "Phone is too long");
    public static readonly Error PhoneInvalidFormat = Error.Validation("identity.phone.invalid", "Phone is invalid");  
    
    public static readonly Error UserBlocked = Error.Forbidden("identity.user.blocked", "User is blocked");
    public static readonly Error UserIsNotActive = Error.Forbidden("identity.user.is_not_active", "User is not active");
    
    public static readonly Error PhoneNotProvided = Error.Validation("identity.phone.not_provided", "Phone is not provided");
    
    public static readonly Error PasswordIsEmpty = Error.Validation("identity.password.empty", "Password cannot be empty");
    
    public static readonly Error VerificationPurposeAndChannelAreNotCompatible = 
        Error.Validation("identity.verification_purpose_and_channel.not_compatible",
        "Verification purpose and channel are not compatible");

    public static readonly Error CodeIsAlreadyUsed = 
        Error.Conflict("identity.code.already_used", "Code is already used");
    
    public static readonly Error VerificationSessionIsCancelled = 
        Error.Conflict("identity.verification_session.cancelled", "Verification session is cancelled");
    
    public static readonly Error VerificationSessionExpired = 
        Error.Validation("identity.verification_session.expired", "Verification session has expired");
    
    public static readonly Error CodeHashIsEmpty = 
        Error.Validation("identity.code_hash.empty", "Code is empty");
    public static readonly Error TooManyFailedAttempts = 
        Error.Conflict("identity.too_many_attempts", "Too many attempts");
    
    public static readonly Error UnsupportedVerificationChannel =
        Error.Validation("identity.unsupported_verification_channel", "User doesn't support this verification channel");

    public static readonly Error RefreshTokenRevoked =
        Error.Unauthorized("identity.refresh_token_revoked", "Refresh token revoked");
    
    public static readonly Error RefreshTokenExpired =
        Error.Unauthorized("identity.refresh_token_expired", "Refresh token has expired");
}