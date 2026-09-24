using Guildly.Common.Domain;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Domain.Repositories;

public interface IIdentityUserRepository : IRepository<IdentityUser>
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsByPhoneAsync(Phone phone, CancellationToken cancellationToken = default);
    
    Task<IdentityUser?> GetUserByEmailAsync(Email email, bool asNoTracking = false, CancellationToken cancellationToken = default);
    
    Task<IdentityUser?> GetUserByIdAsync(Guid userId, bool asNoTracking = false, CancellationToken cancellationToken = default);
}