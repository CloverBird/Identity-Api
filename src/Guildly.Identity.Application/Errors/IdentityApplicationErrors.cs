using Guildly.Common.Results;
using Guildly.Identity.Application.Commands.Login;

namespace Guildly.Identity.Application.Errors;

public static class IdentityApplicationErrors
{
    public static readonly Error UserWithEmailAlreadyExists = 
        Error.Conflict("identity.email.already_exists", "User with this email already exists");
    public static readonly Error UserWithPhoneAlreadyExists =
        Error.Conflict("identity.phone.already_exists", "User with this phone already exists");
    
    public static readonly Error UserNotFound =
        Error.NotFound("identity.user.not_found", "User not found");

    public static readonly Error RegistrationAlreadyConfirmed =
        Error.Conflict("identity.user_registration.already_confirmed", "Registration already confirmed");
    
    public static readonly Error UserBlocked =
        Error.Forbidden("identity.user.blocked", "User is blocked");
    public static readonly Error UserNotActive =
        Error.Forbidden("identity.user.not_active", "User is not active");
    
    public static readonly Error VerificationSessionNotFound =
        Error.NotFound("identity.verification_session.not_found", "Verification session not found");

    public static readonly Error InvalidConfirmationCode =
        Error.Validation("identity.verification_session.invalid_confirmation_code", "Invalid confirmation code");

    public static readonly Error InvalidPassword =
        Error.Validation("identity.invalid_password", "Invalid password");

    public static readonly Error UserChannelIsNotAccessible =
        Error.Validation("identity.user.channel_not_accessible", "User cannot use this channel");

    public static Error InvalidCredentials =
        Error.Unauthorized("identity.invalid_credentials", "Invalid login or password");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("identity.invalid_refresh_token", "Invalid refresh token");
}