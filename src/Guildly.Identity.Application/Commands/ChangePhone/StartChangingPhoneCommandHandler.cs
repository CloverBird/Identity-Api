using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ChangePhone;

internal class StartChangingPhoneCommandHandler : ICommandHandler<StartChangingPhoneCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;
    
    private readonly IIdentityContactsService _identityContactsService;

    public StartChangingPhoneCommandHandler(IIdentityUserService identityUserService, 
                                            IVerificationSessionService verificationSessionService,
                                            IIdentityUnitOfWork identityUnitOfWork,
                                            IPasswordService passwordService, 
                                            IIdentityContactsService identityContactsService)
    {
        _identityUserService = identityUserService;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = identityUnitOfWork;
        _passwordService = passwordService;
        _identityContactsService = identityContactsService;
    }

    public async Task<Result> HandleAsync(StartChangingPhoneCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        //check that user exists
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;
        
        // check password
        var passwordVerificationResult = _passwordService.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordVerificationResult) return IdentityApplicationErrors.InvalidPassword;

        var phoneResult = await _identityContactsService.GetAvailablePhoneAsync(command.Phone, cancellationToken);
        if (phoneResult.IsFailure) return phoneResult.Error;
        
        var startSessionResult = await _verificationSessionService.StartVerificationSessionAsync(
            user.Id,
            phoneResult.Value.Value,
            VerificationPurpose.ChangePhone,
            VerificationChannel.Sms,
            now,
            cancellationToken);
        if (startSessionResult.IsFailure) return startSessionResult.Error;

        // TODO: send notification with code to phone
        var code = startSessionResult.Value.Code;
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}