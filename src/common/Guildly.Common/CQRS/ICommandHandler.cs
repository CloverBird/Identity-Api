using System.Threading;
using System.Threading.Tasks;
using Guildly.Common.Results;

namespace Guildly.Common.CQRS;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}