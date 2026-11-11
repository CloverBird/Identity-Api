using Guildly.Common.Application;

using Guildly.Common.Results;

namespace Guildly.Identity.Application.UnitOfWork;

public interface IIdentityUnitOfWork : IUnitOfWork
{
    Task<Result> TrySaveIdentityUserChangesAsync(CancellationToken cancellationToken = default);
}
