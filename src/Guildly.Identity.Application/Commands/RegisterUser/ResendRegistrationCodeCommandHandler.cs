using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Commands.RegisterUser;

internal class ResendRegistrationCodeCommandHandler : ICommandHandler<ResendRegistrationCodeCommand>
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IVerificationSessionService _verificationSessionService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    public ResendRegistrationCodeCommandHandler(IIdentityUserRepository identityUserRepository,
                                                            IVerificationSessionService verificationSessionService,
                                                            IIdentityUnitOfWork identityUnitOfWork)
    {
        _identityUserRepository = identityUserRepository;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result> HandleAsync(ResendRegistrationCodeCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure) return emailResult.Error;
        
        var user = await _identityUserRepository.GetUserByEmailAsync(emailResult.Value, true, cancellationToken);
        if (user is null) return IdentityApplicationErrors.UserNotFound;

        if (user.Status == UserStatus.Blocked) return IdentityApplicationErrors.UserBlocked;
        
        if (user.EmailConfirmed || user.Status == UserStatus.Active)
        {
            return IdentityApplicationErrors.RegistrationAlreadyConfirmed;
        }
        
        var startSessionResult = await _verificationSessionService.StartVerificationSessionAsync(
            user.Id,
            user.Email.Value,
            VerificationPurpose.RegisterEmail,
            VerificationChannel.Email,
            now,
            cancellationToken);
        if (startSessionResult.IsFailure) return startSessionResult.Error;
        
        // TODO: send notification with code
        var code = startSessionResult.Value.Code;

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}