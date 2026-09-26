using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class RemovePhoneRequestValidator : AbstractValidator<RemovePhoneRequest>
{
    public RemovePhoneRequestValidator()
    {
        this.ValidatePassword(r => r.Password);
    }
}