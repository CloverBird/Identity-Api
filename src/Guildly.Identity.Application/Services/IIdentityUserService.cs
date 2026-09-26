using Guildly.Common.Results;
using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Application.Services;

internal interface IIdentityUserService
{
    // when succeed return existed and active user
    Task<Result<IdentityUser>> GetActiveUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    
    // checks that email is valid and when succeed return existed and active user
    Task<Result<IdentityUser>> GetActiveUserByEmailAsync(string email, CancellationToken cancellationToken = default);
}