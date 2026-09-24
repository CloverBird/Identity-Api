using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Services;

internal class IdentityUserService : IIdentityUserService
{
    private readonly IIdentityUserRepository _identityUserRepository;

    public IdentityUserService(IIdentityUserRepository identityUserRepository)
    {
        _identityUserRepository = identityUserRepository;
    }

    public async Task<Result<IdentityUser>> GetActiveUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _identityUserRepository.GetUserByIdAsync(userId, false, cancellationToken);
        if (user is null)
        {
            return IdentityApplicationErrors.UserNotFound;
        }
        
        return user.Status != UserStatus.Active ? IdentityApplicationErrors.UserNotActive : user;
    }

    public async Task<Result<IdentityUser>> GetActiveUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }
        
        var user = await _identityUserRepository.GetUserByEmailAsync(emailResult.Value, false, cancellationToken);
        if (user is null)
        {
            return IdentityApplicationErrors.UserNotFound;
        }
        
        return user.Status != UserStatus.Active ? IdentityApplicationErrors.UserNotActive : user;
    }
}