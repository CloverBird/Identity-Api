using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Commands.ResetPassword;

internal class StartPasswordResetCommandHandler : ICommandHandler<StartPasswordResetCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    public StartPasswordResetCommandHandler(IIdentityUserService identityUserService,
                                            IVerificationSessionService verificationSessionService,
                                            IIdentityUnitOfWork identityUnitOfWork)
    {
        _identityUserService = identityUserService;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result> HandleAsync(StartPasswordResetCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var userResult = await _identityUserService.GetActiveUserByEmailAsync(command.UserEmail, cancellationToken);
        if (userResult.IsFailure) return IdentityApplicationErrors.UserNotFound;
        var user = userResult.Value;

        var contactResult = user.GetContact(command.Channel);
        if (contactResult.IsFailure) return contactResult.Error;

        var startSessionResult = await _verificationSessionService.StartVerificationSessionAsync(
            user.Id,
            contactResult.Value,
            VerificationPurpose.ResetPassword,
            command.Channel,
            now,
            cancellationToken);
        if (startSessionResult.IsFailure) return startSessionResult.Error;

        //TODO: send code
        var code = startSessionResult.Value.Code;

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}