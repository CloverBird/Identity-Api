using System.Threading;
using System.Threading.Tasks;
using Guildly.Common.Results;

namespace Guildly.Common.CQRS;

public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery
{
    Task<Result<TResult>> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}