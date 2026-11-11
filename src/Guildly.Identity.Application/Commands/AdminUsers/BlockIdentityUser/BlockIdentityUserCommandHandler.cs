using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.AdminUsers.BlockIdentityUser;

internal class BlockIdentityUserCommandHandler(
    IIdentityUserRepository identityUserRepository,
    IRefreshTokenService refreshTokenService,
    IIdentityUnitOfWork identityUnitOfWork)
    : ICommandHandler<BlockIdentityUserCommand, UserStatus>
{
    public async Task<Result<UserStatus>> HandleAsync(
        BlockIdentityUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await identityUserRepository.GetUserByIdAsync(command.UserId, false, cancellationToken);
        if (user is null || user.Role != UserRole.User)
        {
            return IdentityApplicationErrors.UserNotFound;
        }

        user.Block();
        await refreshTokenService.RevokeAllActiveTokensAsync(user.Id, DateTimeOffset.UtcNow, cancellationToken);
        await identityUnitOfWork.SaveChangesAsync(cancellationToken);

        return user.Status;
    }
}
