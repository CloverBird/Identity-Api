using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Commands.ResetPassword;

internal class ConfirmPasswordResetCommandHandler : ICommandHandler<ConfirmPasswordResetCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IRefreshTokenService _refreshTokenService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;

    public ConfirmPasswordResetCommandHandler(IIdentityUserService identityUserService,
                                              IVerificationSessionService verificationSessionService,
                                              IRefreshTokenService refreshTokenService,
                                              IIdentityUnitOfWork identityUnitOfWork,
                                              IPasswordService passwordService)
    {
        _identityUserService = identityUserService;
        _verificationSessionService = verificationSessionService;
        _refreshTokenService = refreshTokenService;
        _identityUnitOfWork = identityUnitOfWork;
        _passwordService = passwordService;
    }

    public async Task<Result> HandleAsync(ConfirmPasswordResetCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var userResult = await _identityUserService.GetActiveUserByEmailAsync(command.UserEmail, cancellationToken);
        if (userResult.IsFailure) return IdentityApplicationErrors.UserNotFound;
        var user = userResult.Value;

        var confirmSessionResult = await _verificationSessionService.ConfirmSessionAsync(
            user.Id,
            command.Code,
            now,
            VerificationPurpose.ResetPassword,
            command.Channel,
            cancellationToken);
        if (confirmSessionResult.IsFailure)
        {
            if (confirmSessionResult.Error == IdentityApplicationErrors.InvalidConfirmationCode)
            {
                await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            return confirmSessionResult.Error;
        }

        var passwordHash = _passwordService.HashPassword(command.NewPassword);
        var result = user.ChangePassword(passwordHash);
        if (result.IsFailure) return result.Error;
        
        // revoke all refresh tokens
        await _refreshTokenService.RevokeAllActiveTokensAsync(user.Id, now, cancellationToken);

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}