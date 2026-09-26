using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;

namespace Guildly.Identity.Application.Commands.Logout;

internal class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IRefreshTokenService _refreshTokenService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    public LogoutCommandHandler(IRefreshTokenService refreshTokenService, 
                                IIdentityUnitOfWork identityUnitOfWork)
    {
        _refreshTokenService = refreshTokenService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result> HandleAsync(LogoutCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var revokeResult = await _refreshTokenService.RevokeTokenAsync(command.Token, now, cancellationToken);
        if (revokeResult.IsFailure)
        {
            return revokeResult.Error;
        }
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}