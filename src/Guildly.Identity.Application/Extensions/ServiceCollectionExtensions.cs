using Guildly.Common.CQRS;
using Guildly.Identity.Application.Commands.AdminUsers.BlockIdentityUser;
using Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;
using Guildly.Identity.Application.Commands.AdminUsers.DeleteIdentityUser;
using Guildly.Identity.Application.Commands.AdminUsers.UnblockIdentityUser;
using Guildly.Identity.Application.Commands.ChangeEmail;
using Guildly.Identity.Application.Commands.ChangePassword;
using Guildly.Identity.Application.Commands.ChangePhone;
using Guildly.Identity.Application.Commands.Login;
using Guildly.Identity.Application.Commands.Logout;
using Guildly.Identity.Application.Commands.RegisterUser;
using Guildly.Identity.Application.Commands.RemovePhone;
using Guildly.Identity.Application.Commands.ResetPassword;
using Guildly.Identity.Application.Commands.RotateToken;
using Guildly.Identity.Application.Queries.AdminUsers;
using Guildly.Identity.Application.Queries.GetMeAsUser;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Guildly.Identity.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IVerificationSessionService, VerificationSessionService>();
        services.AddScoped<IIdentityContactsService, IdentityContactsService>();
        services.AddScoped<IIdentityUserService, IdentityUserService>();
        
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        
        services.AddScoped<ICommandHandler<RegisterUserCommand>, RegisterUserCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmRegistrationCommand>, ConfirmRegistrationCommandHandler>();
        services.AddScoped<ICommandHandler<ResendRegistrationCodeCommand>, ResendRegistrationCodeCommandHandler>();
        
        services.AddScoped<ICommandHandler<StartChangingEmailCommand>, StartChangingEmailCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmChangingEmailCommand>, ConfirmChangingEmailCommandHandler>();
        
        services.AddScoped<ICommandHandler<RemovePhoneCommand>, RemovePhoneCommandHandler>();
        services.AddScoped<ICommandHandler<StartChangingPhoneCommand>, StartChangingPhoneCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmChangingPhoneCommand>, ConfirmChangingPhoneCommandHandler>();
        
        services.AddScoped<ICommandHandler<ChangePasswordCommand>, ChangePasswordCommandHandler>();
        services.AddScoped<ICommandHandler<StartPasswordResetCommand>, StartPasswordResetCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmPasswordResetCommand>, ConfirmPasswordResetCommandHandler>();
        
        services.AddScoped<ICommandHandler<LoginUserCommand, LoginUserCommandResult>, LoginUserCommandHandler>();
        services.AddScoped<ICommandHandler<CreateIdentityUserCommand, CreateIdentityUserCommandResult>, CreateIdentityUserCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteIdentityUserCommand>, DeleteIdentityUserCommandHandler>();
        services.AddScoped<ICommandHandler<BlockIdentityUserCommand, UserStatus>, BlockIdentityUserCommandHandler>();
        services.AddScoped<ICommandHandler<UnblockIdentityUserCommand, UserStatus>, UnblockIdentityUserCommandHandler>();
        
        services.AddScoped<ICommandHandler<RotateTokenCommand, RotateTokenCommandResult>, RotateTokenCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutFromAllDevicesCommand>, LogoutFromAllDevicesCommandHandler>();

        services.AddScoped<IQueryHandler<GetMeAsUserQuery, GetMeAsUserQueryResult>, GetMeAsUserQueryHandler>();
        services.AddScoped<IQueryHandler<GetIdentityUsersQuery, GetIdentityUsersQueryResult>, GetIdentityUsersQueryHandler>();
        services.AddScoped<IQueryHandler<GetIdentityUserQuery, GetIdentityUserQueryResult>, GetIdentityUserQueryHandler>();
        
        return services;
    } 
}
