using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class ConfirmRegistrationRequestValidator : AbstractValidator<ConfirmRegistrationRequest>
{
    public ConfirmRegistrationRequestValidator()
    {
        this.ValidateEmail(r => r.Email);

        this.ValidateCode(r => r.Code);
    }
}