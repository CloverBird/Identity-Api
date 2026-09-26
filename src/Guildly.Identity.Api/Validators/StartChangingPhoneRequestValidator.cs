using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

internal class StartChangingPhoneRequestValidator : AbstractValidator<StartChangingPhoneRequest>
{
    public StartChangingPhoneRequestValidator()
    {
        this.ValidatePhone(r => r.NewPhone);
        
        this.ValidatePassword(r => r.Password);
    }
}