using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Guildly.Identity.Database.Migration.Job.Services;

public class MigrationService : IMigrationService
{
    private readonly IdentityDbContext _dbContext;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(IdentityDbContext dbContext, ILogger<MigrationService> logger)
    {
        _dbContext = dbContext;

        _logger = logger;
    }

    public async Task ApplyMigrationAsync()
    {
        _logger.LogInformation("Apply database migration ...");

        await ApplyMigration();
    }

    protected async Task ApplyMigration()
    {
        var migrationWasApplied = false;

        var attempts = 0;
        while (attempts++ < 30 && !migrationWasApplied)
        {
            try
            {
                await _dbContext.Database.OpenConnectionAsync();

                await _dbContext.Database.MigrateAsync();

                _logger.LogInformation("Migration was applied");

                migrationWasApplied = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fail attempt: {message}, {stack}", ex.Message, ex.StackTrace);

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            finally
            {
                try
                {
                    await _dbContext.Database.CloseConnectionAsync();
                }
                catch { /* ignore */ }
            }
        }

        if (!migrationWasApplied)
        {
            throw new Exception("Database is unavailable.");
        }
    }
}
