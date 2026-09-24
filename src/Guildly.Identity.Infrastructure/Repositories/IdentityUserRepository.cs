using Guildly.Common.Database.Repository;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;
using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Identity.Infrastructure.Repositories;

internal class IdentityUserRepository : Repository<IdentityUser, IdentityDbContext>, IIdentityUserRepository
{
    public IdentityUserRepository(IdentityDbContext context) 
        : base(context) { }

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return DbContext.IdentityUsers.AnyAsync(
            u => u.Email.NormalizedValue == email.NormalizedValue,
            cancellationToken);
    }

    public Task<bool> ExistsByPhoneAsync(Phone phone, CancellationToken cancellationToken = default)
    {
        return DbContext.IdentityUsers.AnyAsync(
            u => u.Phone == phone,
            cancellationToken);
    }

    public Task<IdentityUser?> GetUserByEmailAsync(Email email, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.IdentityUsers.AsNoTracking() : DbContext.IdentityUsers;
        
        return query.FirstOrDefaultAsync(
            u => u.Email.NormalizedValue == email.NormalizedValue,
            cancellationToken);
    }

    public Task<IdentityUser?> GetUserByIdAsync(Guid userId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.IdentityUsers.AsNoTracking() : DbContext.IdentityUsers;
        
        return query.FirstOrDefaultAsync(
            u => u.Id == userId,
            cancellationToken);
    }
}