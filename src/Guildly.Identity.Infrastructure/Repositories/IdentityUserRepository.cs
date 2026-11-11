using Guildly.Common.Database.Repository;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
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

    public async Task<(IReadOnlyList<IdentityUser> Users, int TotalCount)> GetIdentityUsersAsync(
        UserRole role, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = DbContext.IdentityUsers.AsNoTracking().Where(user => user.Role == role);
        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query.OrderByDescending(user => user.CreatedAt)
                               .ThenByDescending(user => user.Id)
                               .Skip(skip)
                               .Take(take)
                               .ToListAsync(cancellationToken);

        return (users, totalCount);
    }
}
