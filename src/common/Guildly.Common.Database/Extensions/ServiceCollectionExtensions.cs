using Guildly.Common.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildly.Common.Database.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPostgresDatabase<TDbContext>(
        this IServiceCollection services, 
        DatabaseConfiguration configuration,
        string schema)
        where TDbContext : DbContext
    {
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(configuration.ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schema);
            });
            options.UseSnakeCaseNamingConvention();
        });
        
        return services;
    }
}