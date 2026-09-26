using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ChangePhone;

internal class ConfirmChangingPhoneCommandHandler : ICommandHandler<ConfirmChangingPhoneCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    private readonly IIdentityContactsService _identityContactsService;
    
    public ConfirmChangingPhoneCommandHandler(IIdentityUserService identityUserService,
                                              IVerificationSessionService verificationSessionService,
                                              IIdentityUnitOfWork unitOfWork,
                                              IIdentityContactsService identityContactsService)
    {
        _identityUserService = identityUserService;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = unitOfWork;
        _identityContactsService = identityContactsService;
    }
    
    public async Task<Result> HandleAsync(ConfirmChangingPhoneCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var confirmSessionResult = await _verificationSessionService.ConfirmSessionAsync(
            user.Id,
            command.Code,
            now,
            VerificationPurpose.ChangePhone,
            VerificationChannel.Sms,
            cancellationToken);
        if (confirmSessionResult.IsFailure)
        {
            if (confirmSessionResult.Error == IdentityApplicationErrors.InvalidConfirmationCode)
            {
                await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            return confirmSessionResult.Error;
        }
        
        var phoneResult = await _identityContactsService.GetAvailablePhoneAsync(confirmSessionResult.Value.SentTo, cancellationToken);
        if (phoneResult.IsFailure)
        {
            await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
            return phoneResult.Error;
        }

        var changingPhoneResult = user.ChangePhone(confirmSessionResult.Value.SentTo);
        if (changingPhoneResult.IsFailure) return changingPhoneResult.Error;
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}