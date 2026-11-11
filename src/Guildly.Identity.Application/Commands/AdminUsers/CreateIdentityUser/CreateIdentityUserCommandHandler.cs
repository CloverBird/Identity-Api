using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;

internal class CreateIdentityUserCommandHandler(
    IIdentityContactsService identityContactsService,
    IPasswordService passwordService,
    IIdentityUserRepository identityUserRepository,
    IIdentityUnitOfWork identityUnitOfWork)
    : ICommandHandler<CreateIdentityUserCommand, CreateIdentityUserCommandResult>
{
    public async Task<Result<CreateIdentityUserCommandResult>> HandleAsync(
        CreateIdentityUserCommand command, CancellationToken cancellationToken = default)
    {
        var email = await identityContactsService.GetAvailableEmailAsync(command.Email, cancellationToken);
        if (email.IsFailure)
        {
            return email.Error;
        }

        var passwordHash = passwordService.HashPassword(command.Password);
        var userResult = IdentityUser.Create(
            Guid.NewGuid(), email.Value.Value, passwordHash, UserRole.User, DateTimeOffset.UtcNow);
        if (userResult.IsFailure)
        {
            return userResult.Error;
        }

        var user = userResult.Value;
        identityUserRepository.Add(user);

        var saveResult = await identityUnitOfWork.TrySaveIdentityUserChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            return saveResult.Error;
        }

        return new CreateIdentityUserCommandResult(
            user.Id, user.Email.Value, user.Status, user.EmailConfirmed, user.CreatedAt);
    }
}
