using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.AdminUsers.UnblockIdentityUser;

internal class UnblockIdentityUserCommandHandler(
    IIdentityUserRepository identityUserRepository,
    IIdentityUnitOfWork identityUnitOfWork)
    : ICommandHandler<UnblockIdentityUserCommand, UserStatus>
{
    public async Task<Result<UserStatus>> HandleAsync(
        UnblockIdentityUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await identityUserRepository.GetUserByIdAsync(command.UserId, false, cancellationToken);
        if (user is null || user.Role != UserRole.User)
        {
            return IdentityApplicationErrors.UserNotFound;
        }

        user.Unblock();
        await identityUnitOfWork.SaveChangesAsync(cancellationToken);

        return user.Status;
    }
}
