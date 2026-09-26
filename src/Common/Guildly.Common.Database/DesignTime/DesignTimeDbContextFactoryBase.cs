using System;
using System.IO;
using Guildly.Common.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Guildly.Common.Database.DesignTime;

public abstract class DesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly string _startupLocation;
    protected abstract string Schema { get; }

    protected DesignTimeDbContextFactoryBase(string startupLocation = @"..\Guildly.Identity.Api")
    {
        _startupLocation = startupLocation;
    }

    public TDbContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "local";
        
        var startupDirectory = Path.GetFullPath(_startupLocation);
        
        var configuration = new ConfigurationBuilder()
            .SetBasePath(startupDirectory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        
        var databaseConfiguration = configuration.GetRequiredSection("Database")
                                                 .Get<DatabaseConfiguration>();

        optionsBuilder.UseNpgsql(databaseConfiguration!.ConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schema);
        });
        optionsBuilder.UseSnakeCaseNamingConvention();
        
        return CreateInstance(optionsBuilder.Options);
    }

    protected abstract TDbContext CreateInstance(DbContextOptions<TDbContext> options);
}
