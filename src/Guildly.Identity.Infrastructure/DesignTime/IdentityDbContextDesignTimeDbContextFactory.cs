using Guildly.Common.Database.DesignTime;
using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Identity.Infrastructure.DesignTime;

internal class IdentityDbContextDesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<IdentityDbContext>
{
    protected override string Schema => "identity";
    
    protected override IdentityDbContext CreateInstance(DbContextOptions<IdentityDbContext> options)
    {
        return new IdentityDbContext(options);
    }
}