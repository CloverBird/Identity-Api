using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class ConfirmCodeRequestValidator : AbstractValidator<ConfirmCodeRequest>
{
    public ConfirmCodeRequestValidator()
    {
        this.ValidateCode(r => r.Code);
    }
}