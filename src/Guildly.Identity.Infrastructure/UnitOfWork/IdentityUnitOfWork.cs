using Guildly.Common.Database.UnitOfWork;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Infrastructure.DbContexts;

namespace Guildly.Identity.Infrastructure.UnitOfWork;

internal class IdentityUnitOfWork : UnitOfWork<IdentityDbContext>, IIdentityUnitOfWork
{
    public IdentityUnitOfWork(IdentityDbContext dbContext) 
        : base(dbContext) { }
}