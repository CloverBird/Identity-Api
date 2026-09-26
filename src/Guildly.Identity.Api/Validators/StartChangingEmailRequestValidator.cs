using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class StartChangingEmailRequestValidator : AbstractValidator<StartChangingEmailRequest>
{
    public StartChangingEmailRequestValidator()
    {
        this.ValidateEmail(r => r.NewEmail);
        
        this.ValidatePassword(r => r.Password);
    }
}