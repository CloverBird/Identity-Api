using Guildly.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Common.Database.Repository;

public abstract class Repository<TEntity, TDbContext> : IRepository<TEntity>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext DbContext;

    public Repository(TDbContext context)
    {
        DbContext = context;
    }
    
    public void Add(TEntity entity)
    {
        DbContext.Set<TEntity>().Add(entity);
    }
    
    public void Remove(TEntity entity)
    {
        DbContext.Set<TEntity>().Remove(entity);
    }
}