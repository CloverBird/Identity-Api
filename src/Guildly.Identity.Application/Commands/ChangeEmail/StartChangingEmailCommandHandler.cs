using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ChangeEmail;

internal class StartChangingEmailCommandHandler : ICommandHandler<StartChangingEmailCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;
    
    private readonly IIdentityContactsService _identityContactsService;

    public StartChangingEmailCommandHandler(IIdentityUserService identityUserService, 
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

    public async Task<Result> HandleAsync(StartChangingEmailCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var passwordVerified = _passwordService.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordVerified) return IdentityApplicationErrors.InvalidPassword;

        var emailResult = await _identityContactsService.GetAvailableEmailAsync(command.NewEmail, cancellationToken);
        if (emailResult.IsFailure) return emailResult.Error;
        
        var startSessionResult = await _verificationSessionService.StartVerificationSessionAsync(
            command.UserId,
            emailResult.Value.Value,
            VerificationPurpose.ChangeEmail,
            VerificationChannel.Email,
            now,
            cancellationToken);
        if (startSessionResult.IsFailure) return startSessionResult.Error;
        
        var code = startSessionResult.Value.Code;
        //TODO: send notification with code

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}