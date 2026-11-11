using FluentValidation;
using Guildly.Identity.Api.Requests;

namespace Guildly.Identity.Api.Validators;

public class CreateIdentityUserRequestValidator : AbstractValidator<CreateIdentityUserRequest>
{
    public CreateIdentityUserRequestValidator()
    {
        this.ValidateEmail(request => request.Email);
        this.ValidatePassword(request => request.Password);
    }
}
