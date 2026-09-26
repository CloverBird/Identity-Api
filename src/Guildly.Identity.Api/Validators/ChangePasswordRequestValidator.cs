using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        this.ValidatePassword(r => r.NewPassword);
        this.ValidatePassword(r => r.OldPassword);
    }
}