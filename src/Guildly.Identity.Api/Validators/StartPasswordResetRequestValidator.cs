using FluentValidation;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Validators;

public class StartPasswordResetRequestValidator : AbstractValidator<StartPasswordResetRequest>
{
    public StartPasswordResetRequestValidator()
    {
        When(r => r.Channel == VerificationChannel.Email, 
            () => this.ValidateEmail(r => r.Target));
        When(r => r.Channel == VerificationChannel.Sms, 
            () => this.ValidatePhone(r => r.Target));
        
        RuleFor(r => r.Channel)
            .IsInEnum().WithMessage("Invalid channel");
    }
}