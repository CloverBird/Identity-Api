using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;

namespace Guildly.Identity.Application.Commands.Logout;

internal class LogoutFromAllDevicesCommandHandler : ICommandHandler<LogoutFromAllDevicesCommand>
{
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;

    public LogoutFromAllDevicesCommandHandler(IRefreshTokenService refreshTokenService,
                                              IIdentityUnitOfWork identityUnitOfWork)
    {
        _refreshTokenService = refreshTokenService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result> HandleAsync(LogoutFromAllDevicesCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        await _refreshTokenService.RevokeAllActiveTokensAsync(command.UserId, now, cancellationToken);

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}