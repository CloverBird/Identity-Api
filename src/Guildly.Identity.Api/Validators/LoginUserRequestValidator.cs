using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        // not strong rules for login
        RuleFor(r => r.Email)
            .NotEmpty().WithMessage("Email is required");
        
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}