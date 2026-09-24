using Guildly.Common.Database.Configurations;
using Guildly.Common.Database.Extensions;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Infrastructure.DbContexts;
using Guildly.Identity.Infrastructure.Repositories;
using Guildly.Identity.Infrastructure.Services;
using Guildly.Identity.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using IdentityUser = Guildly.Identity.Domain.Entities.IdentityUser;

namespace Guildly.Identity.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, DatabaseConfiguration databaseConfiguration)
    {
        services.AddPostgresDatabase<IdentityDbContext>(databaseConfiguration, IdentityDbContext.Schema);

        services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();
        services.AddScoped<IVerificationSessionRepository, VerificationSessionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        
        services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddSingleton<IVerificationCodeService, VerificationCodeService>();
        
        services.AddSingleton<IAccessTokenProvider, JwtAccessTokenProvider>();
        services.AddSingleton<IRefreshTokenProvider, RefreshTokenProvider>();
        
        return services;
    }
}