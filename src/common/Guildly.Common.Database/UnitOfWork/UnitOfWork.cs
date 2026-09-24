using System.Threading;
using System.Threading.Tasks;
using Guildly.Common.Application;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Common.Database.UnitOfWork;

public abstract class UnitOfWork<TDbContext> : IUnitOfWork
    where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;

    protected UnitOfWork(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return DbContext.SaveChangesAsync(cancellationToken);
    }
}