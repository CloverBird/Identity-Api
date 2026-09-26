using Guildly.Identity.Api.Endpoints.ChangeEmail;
using Guildly.Identity.Api.Endpoints.ChangePhone;
using Guildly.Identity.Api.Endpoints.Registration;
using Guildly.Identity.Api.Endpoints.ResetPassword;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth")
                       .WithTags("Auth")
                       .RequireAuthorization();
        
        group.MapRegisterUser()
             .MapConfirmRegistration()
             .MapResendRegistrationCode();
             
        group.MapStartChangingEmail()
             .MapConfirmChangingEmail();
        
        group.MapStartChangingPhone()
             .MapConfirmChangingPhone()
             .MapRemovePhone();

        group.MapStartPasswordReset()
             .MapConfirmPasswordReset()
             .MapChangePassword();

        group.MapGetIdentityUser();

        group.MapLogin()
             .MapRotateToken()
             .MapLogout()
             .MapLogoutFromAllDevices();
        
        return app;
    }
}