using System.Security.Claims;
using System.Text;
using FluentValidation;
using Guildly.Common.Database.Configurations;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Api.Validators;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Extensions;
using Guildly.Identity.Infrastructure.Configurations;
using Guildly.Identity.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Guildly.Identity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddTransient<IValidator<RegisterUserRequest>, RegisterUserRequestValidator>();
        services.AddTransient<IValidator<ConfirmRegistrationRequest>, ConfirmRegistrationRequestValidator>();
        services.AddTransient<IValidator<ResendRegistrationCodeRequest>, ResendRegistrationCodeRequestValidator>();
        
        services.AddTransient<IValidator<ConfirmCodeRequest>, ConfirmCodeRequestValidator>();
        
        services.AddTransient<IValidator<StartChangingEmailRequest>, StartChangingEmailRequestValidator>();
        services.AddTransient<IValidator<StartChangingPhoneRequest>, StartChangingPhoneRequestValidator>();
        services.AddTransient<IValidator<RemovePhoneRequest>, RemovePhoneRequestValidator>();
        
        services.AddTransient<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
        
        services.AddTransient<IValidator<StartPasswordResetRequest>, StartPasswordResetRequestValidator>();
        services.AddTransient<IValidator<ConfirmPasswordResetRequest>, ConfirmPasswordResetRequestValidator>();
        
        services.AddTransient<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
        
        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, JwtConfiguration jwtConfiguration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtConfiguration.Issuer,
                    ValidAudience = jwtConfiguration.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.SecretKey)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        services.AddAuthorization(); // TODO: setup policies/roles/requirements
        
        return services;
    }
}