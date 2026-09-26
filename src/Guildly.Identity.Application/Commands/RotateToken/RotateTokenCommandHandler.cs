using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.RotateToken;

internal class RotateTokenCommandHandler : ICommandHandler<RotateTokenCommand, RotateTokenCommandResult>
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IAccessTokenProvider _accessTokenProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    public RotateTokenCommandHandler(IIdentityUserRepository identityUserRepository,
                                     IAccessTokenProvider accessTokenProvider,
                                     IRefreshTokenService refreshTokenService, 
                                     IIdentityUnitOfWork identityUnitOfWork)
    {
        _identityUserRepository = identityUserRepository;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenService = refreshTokenService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result<RotateTokenCommandResult>> HandleAsync(RotateTokenCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var refreshToken = await _refreshTokenService.GetRefreshTokenAsync(command.RefreshToken, false, cancellationToken);
        if (refreshToken is null)
        {
            return IdentityApplicationErrors.InvalidRefreshToken;
        }

        var refreshTokenIsActive = refreshToken.EnsureIsActive(now);
        if (refreshTokenIsActive.IsFailure)
        {
            return refreshTokenIsActive.Error;
        }

        var user = await _identityUserRepository.GetUserByIdAsync(refreshToken.UserId, true, cancellationToken);
        if (user is null)
        {
            return IdentityApplicationErrors.UserNotFound;
        }
        if (user.Status != UserStatus.Active)
        {
            return IdentityApplicationErrors.UserNotActive;
        }
        
        refreshToken.Revoke(now);

        var newRefreshToken = _refreshTokenService.CreateRefreshToken(user, now, refreshToken.RememberMe);

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        var accessToken = _accessTokenProvider.GenerateAccessToken(user);

        return new RotateTokenCommandResult(
            accessToken.Token, accessToken.ExpiresAt,
            newRefreshToken.Token, newRefreshToken.ExpiresAt);
    }
}