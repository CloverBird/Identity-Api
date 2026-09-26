using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ChangeEmail;

internal class ConfirmChangingEmailCommandHandler : ICommandHandler<ConfirmChangingEmailCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IIdentityContactsService _identityContactsService;

    public ConfirmChangingEmailCommandHandler(IIdentityUserService identityUserService,
                                              IVerificationSessionService verificationSessionService,
                                              IIdentityUnitOfWork unitOfWork, 
                                              IIdentityContactsService identityContactsService)
    {
        _identityUserService = identityUserService;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = unitOfWork;
        _identityContactsService = identityContactsService;
    }

    public async Task<Result> HandleAsync(ConfirmChangingEmailCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var confirmSessionResult = await _verificationSessionService.ConfirmSessionAsync(
            user.Id,
            command.Code,
            now,
            VerificationPurpose.ChangeEmail,
            VerificationChannel.Email,
            cancellationToken);
        if (confirmSessionResult.IsFailure)
        {
            if (confirmSessionResult.Error == IdentityApplicationErrors.InvalidConfirmationCode)
            {
                await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            return confirmSessionResult.Error;
        }
        
        var emailResult = await _identityContactsService.GetAvailableEmailAsync(confirmSessionResult.Value.SentTo, cancellationToken);
        if (emailResult.IsFailure)
        {
            await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
            return emailResult.Error;
        }
        
        var changingEmailResult = user.ChangeEmail(confirmSessionResult.Value.SentTo);
        if (changingEmailResult.IsFailure)
        {
            throw new InvalidOperationException(
                $"Verification session {confirmSessionResult.Value.Id} contains invalid email.");
        }
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}