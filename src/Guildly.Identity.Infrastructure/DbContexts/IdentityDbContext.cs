using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Infrastructure.EntityTypeConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Identity.Infrastructure.DbContexts;

public class IdentityDbContext : DbContext
{
    public const string Schema = "identity";

    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();
    public DbSet<VerificationSession> VerificationSessions => Set<VerificationSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        
        modelBuilder.ApplyConfiguration(new IdentityUserConfiguration());
        modelBuilder.ApplyConfiguration(new VerificationSessionConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}