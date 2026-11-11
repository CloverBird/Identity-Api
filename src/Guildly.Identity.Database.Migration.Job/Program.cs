using System.Reflection;
using Guildly.Common.Database.Configurations;
using Guildly.Identity.Database.Migration.Job.HostedServices;
using Guildly.Identity.Database.Migration.Job.Services;
using Guildly.Identity.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();
    
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    // Note: calculate path to the folder with executable assembly
    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

    builder.Configuration.SetBasePath(path!)
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{environmentName}.json", true, true)
        .AddEnvironmentVariables();

    var databaseConfiguration = builder.Configuration.GetSection("Database").Get<DatabaseConfiguration>();
    builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Database"));

    builder.Services.AddScoped<IMigrationService, MigrationService>();

    builder.Services.AddInfrastructureServices(databaseConfiguration!);
    builder.Services.AddHostedService<MigrationHostedService>();

    var app = builder.Build();

    await app.StartAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
