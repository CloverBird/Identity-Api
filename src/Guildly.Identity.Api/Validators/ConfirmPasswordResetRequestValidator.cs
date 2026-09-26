using FluentValidation;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Validators;

public class ConfirmPasswordResetRequestValidator : AbstractValidator<ConfirmPasswordResetRequest>
{
    public ConfirmPasswordResetRequestValidator()
    {
        When(r => r.Channel == VerificationChannel.Email, 
            () => this.ValidateEmail(r => r.Target));
        When(r => r.Channel == VerificationChannel.Sms, 
            () => this.ValidatePhone(r => r.Target));
        
        this.ValidatePassword(r => r.NewPassword);

        this.ValidateCode(r => r.Code);
        
        RuleFor(c => c.Channel).
            IsInEnum().WithMessage("Invalid channel");
    }
}