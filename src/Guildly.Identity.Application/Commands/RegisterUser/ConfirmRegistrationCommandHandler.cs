using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Commands.RegisterUser;

internal class ConfirmRegistrationCommandHandler : ICommandHandler<ConfirmRegistrationCommand>
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IVerificationSessionService _verificationSessionService;
    
    //private readonly IUsersModuleService _usersModuleService;

    public ConfirmRegistrationCommandHandler(IIdentityUserRepository identityUserRepository,
                                             IIdentityUnitOfWork identityUnitOfWork,
                                             IVerificationSessionService verificationSessionService)
    {
        _identityUserRepository = identityUserRepository;
        _identityUnitOfWork = identityUnitOfWork;
        _verificationSessionService = verificationSessionService;
    }

    //TODO: user activation and changes in identity module should be in the same transaction
    public async Task<Result> HandleAsync(ConfirmRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure) return emailResult.Error;
        
        var user = await _identityUserRepository.GetUserByEmailAsync(emailResult.Value, false, cancellationToken);
        if (user is null) return IdentityApplicationErrors.UserNotFound;

        if (user.EmailConfirmed || user.Status == UserStatus.Active)
        {
            return IdentityApplicationErrors.RegistrationAlreadyConfirmed;
        }

        if (user.Status == UserStatus.Blocked)
        {
            return IdentityApplicationErrors.UserBlocked;
        }

        var confirmSessionResult = await _verificationSessionService.ConfirmSessionAsync(
            user.Id,
            command.Code,
            now,
            VerificationPurpose.RegisterEmail,
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
        
        var confirmationResult = user.ConfirmEmail();
        if (confirmationResult.IsFailure) return confirmationResult.Error;

        // NOTE: this method should activate user in users api
        // var activationUserResult = await _usersModuleService.ActivateUser(user.Id, cancellationToken);
        // if (activationUserResult.IsFailure) return activationUserResult.Error;
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}