using Guildly.Identity.Api.Endpoints.AdminUsers;
using static Guildly.Identity.Api.Extensions.ServiceCollectionExtensions;

namespace Guildly.Identity.Api.Endpoints;

internal static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/users")
                       .WithTags("Admin Users");
                       //.RequireAuthorization(AdminOnlyPolicy); // Note: disable for simplicity testing frontend

        group.MapGetIdentityUsers()
             .MapGetIdentityUser()
             .MapCreateIdentityUser()
             .MapDeleteIdentityUser()
             .MapBlockIdentityUser()
             .MapUnblockIdentityUser();

        return app;
    }
}
