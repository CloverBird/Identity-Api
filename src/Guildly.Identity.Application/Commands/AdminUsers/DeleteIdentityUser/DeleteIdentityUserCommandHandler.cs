using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.AdminUsers.DeleteIdentityUser;

internal class DeleteIdentityUserCommandHandler(
    IIdentityUserRepository identityUserRepository,
    IIdentityUnitOfWork identityUnitOfWork)
    : ICommandHandler<DeleteIdentityUserCommand>
{
    public async Task<Result> HandleAsync(
        DeleteIdentityUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await identityUserRepository.GetUserByIdAsync(command.UserId, false, cancellationToken);
        if (user is null || user.Role != UserRole.User)
        {
            return IdentityApplicationErrors.UserNotFound;
        }

        identityUserRepository.Remove(user);
        await identityUnitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}
