using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Commands.ChangePassword;

internal class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IRefreshTokenService _refreshTokenService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;

    public ChangePasswordCommandHandler(IIdentityUserService identityUserService,
                                        IVerificationSessionService verificationSessionService,
                                        IRefreshTokenService refreshTokenService,
                                        IIdentityUnitOfWork identityUnitOfWork,
                                        IPasswordService passwordService)
    {
        _identityUserService = identityUserService;
        _identityUnitOfWork = identityUnitOfWork;
        _passwordService = passwordService;
        _refreshTokenService = refreshTokenService;
        _verificationSessionService = verificationSessionService;
    }

    public async Task<Result> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var oldPasswordVerification = _passwordService.VerifyPassword(command.OldPassword, user.PasswordHash);
        if (!oldPasswordVerification) return IdentityApplicationErrors.InvalidPassword;

        var newPasswordHash = _passwordService.HashPassword(command.NewPassword);
        
        var changePasswordResult = user.ChangePassword(newPasswordHash);
        if (changePasswordResult.IsFailure) return changePasswordResult.Error;

        // if password was changed cancel all ResetPassword sessions
        await _verificationSessionService.CancelActiveVerificationSessionsAsync(
            user.Id,
            VerificationPurpose.ResetPassword,
            now,
            cancellationToken);
        
        // revoke all refresh tokens
        await _refreshTokenService.RevokeAllActiveTokensAsync(user.Id, now, cancellationToken);
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}