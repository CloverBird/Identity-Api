namespace Guildly.Identity.Database.Migration.Job.Services;

public interface IMigrationService
{
    Task ApplyMigrationAsync();
}
