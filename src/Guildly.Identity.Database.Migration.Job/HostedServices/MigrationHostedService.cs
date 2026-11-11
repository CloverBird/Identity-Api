using Guildly.Identity.Database.Migration.Job.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Guildly.Identity.Database.Migration.Job.HostedServices;

public class MigrationHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public MigrationHostedService(IServiceScopeFactory scopeFactory,
                                  IHostApplicationLifetime applicationLifetime)
    {
        _scopeFactory = scopeFactory;
        _applicationLifetime = applicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var migrationService = scope.ServiceProvider.GetRequiredService<IMigrationService>();
        await migrationService.ApplyMigrationAsync();

        _applicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
