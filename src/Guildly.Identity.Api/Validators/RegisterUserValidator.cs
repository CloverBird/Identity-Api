using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 32;

    private const int MinDisplayNameLength = 3;
    private const int MaxDisplayNameLength = 64;

    public RegisterUserRequestValidator()
    {
        this.ValidateEmail(r => r.Email);
        
        this.ValidatePassword(r => r.Password);

        RuleFor(r => r.Username)
            .Custom((username, context) =>
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    context.AddFailure("Username", "Username is required.");
                    return;
                }

                var normalizedUsername = username.Trim();

                if (normalizedUsername.Length < MinUsernameLength)
                {
                    context.AddFailure("Username", "Username is too short.");
                }

                if (normalizedUsername.Length > MaxUsernameLength)
                {
                    context.AddFailure("Username", "Username is too long.");
                }

                if (!IsLatinLetter(normalizedUsername[0]))
                {
                    context.AddFailure("Username", "Username must start with a latin letter.");
                }

                if (!IsLatinLetterOrDigit(normalizedUsername[^1]))
                {
                    context.AddFailure("Username", "Username must end with a latin letter or digit.");
                }

                if (HasConsecutiveSpecialSymbols(normalizedUsername))
                {
                    context.AddFailure("Username", "Username cannot contain consecutive special symbols.");
                }

                if (normalizedUsername.Any(c => !IsLatinLetterOrDigit(c) && !IsAllowedSpecialSymbol(c)))
                {
                    context.AddFailure("Username", "Username can contain only latin letters, digits, '_', '-' and '.'.");
                }
            });

        RuleFor(r => r.DisplayName)
            .Custom((displayName, context) =>
            {
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    return;
                }

                var normalizedDisplayName = displayName.Trim();

                if (normalizedDisplayName.Length < MinDisplayNameLength)
                {
                    context.AddFailure("DisplayName", "Display name is too short.");
                }

                if (normalizedDisplayName.Length > MaxDisplayNameLength)
                {
                    context.AddFailure("DisplayName", "Display name is too long.");
                }

                if (normalizedDisplayName.Any(char.IsControl))
                {
                    context.AddFailure("DisplayName", "Display name cannot contain control characters.");
                }
            });
    }

    private static bool HasConsecutiveSpecialSymbols(string value)
    {
        for (var i = 0; i < value.Length - 1; i++)
        {
            if (IsAllowedSpecialSymbol(value[i]) &&
                IsAllowedSpecialSymbol(value[i + 1]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsAllowedSpecialSymbol(char c)
    {
        return c is '_' or '-' or '.';
    }

    private static bool IsLatinLetter(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
    }

    private static bool IsLatinLetterOrDigit(char c)
    {
        return IsLatinLetter(c) || char.IsDigit(c);
    }
}