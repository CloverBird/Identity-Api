using Guildly.Common.Database.UnitOfWork;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Guildly.Identity.Infrastructure.UnitOfWork;

internal class IdentityUnitOfWork : UnitOfWork<IdentityDbContext>, IIdentityUnitOfWork
{
    public IdentityUnitOfWork(IdentityDbContext dbContext) 
        : base(dbContext) { }

    public async Task<Result> TrySaveIdentityUserChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_identity_user_normalized_email"
        })
        {
            return IdentityApplicationErrors.UserWithEmailAlreadyExists;
        }
    }
}
