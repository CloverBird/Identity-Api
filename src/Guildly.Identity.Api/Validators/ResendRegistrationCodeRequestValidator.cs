using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class ResendRegistrationCodeRequestValidator : AbstractValidator<ResendRegistrationCodeRequest>
{
    public ResendRegistrationCodeRequestValidator()
    {
        this.ValidateEmail(r => r.Email);
    }
}