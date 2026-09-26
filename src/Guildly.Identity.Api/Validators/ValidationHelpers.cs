using System.Linq.Expressions;
using FluentValidation;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Validators;

public static class ValidationHelpers
{
    private const int MaxEmailLength = 256;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 128;
    private const int MinPhoneLength = 7;
    private const int MaxPhoneLength = 32;
    
    public static void ValidateEmail<T>(this AbstractValidator<T> validator, 
                                        Expression<Func<T, string>> emailProperty)
    {
        validator.RuleFor(emailProperty)
            .NotEmpty()
                .WithMessage("Email is required.")
            .MaximumLength(MaxEmailLength)
                .WithMessage($"Email cannot be longer than {MaxEmailLength} characters.")
            .EmailAddress()
                .WithMessage("Email is invalid.");
    }

    public static void ValidatePhone<T>(this AbstractValidator<T> validator,
                                        Expression<Func<T, string>> phoneProperty)
    {
        validator.RuleFor(phoneProperty)
            .Custom((phone, context) =>
            {
                var normalizedPhone = phone.Trim();

                if (string.IsNullOrWhiteSpace(normalizedPhone))
                {
                    context.AddFailure("Phone is required.");
                    return;
                }

                if (normalizedPhone.Length < MinPhoneLength)
                {
                    context.AddFailure("Phone is too short.");
                    return;
                }

                if (normalizedPhone.Length > MaxPhoneLength)
                {
                    context.AddFailure("Phone is too long.");
                    return;
                }

                var charsToCheck = normalizedPhone.StartsWith('+')
                    ? normalizedPhone[1..]
                    : normalizedPhone;

                if (charsToCheck.Any(x => !char.IsDigit(x)))
                {
                    context.AddFailure("Phone has invalid format.");
                }
            });
    }

    public static void ValidatePassword<T>(this AbstractValidator<T> validator,
                                           Expression<Func<T, string>> passwordProperty)
    {
        validator.RuleFor(passwordProperty)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(MinPasswordLength)
                .WithMessage($"Password must be at least {MinPasswordLength} characters.")
            .MaximumLength(MaxPasswordLength)
                .WithMessage($"Password cannot be longer than {MaxPasswordLength} characters.");
    }

    public static void ValidateCode<T>(this AbstractValidator<T> validator,
                                       Expression<Func<T, string>> codeProperty)
    {
        validator.RuleFor(codeProperty)
            .NotEmpty().WithMessage("Code is required");
    }
}