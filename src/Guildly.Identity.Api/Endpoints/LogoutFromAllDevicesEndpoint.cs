using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Identity.Application.Commands.Logout;
using Microsoft.AspNetCore.Http.HttpResults;

using static Guildly.Identity.Api.Services.HttpContextHelpers;

namespace Guildly.Identity.Api.Endpoints;

internal static class LogoutFromAllDevicesEndpoint
{
    public static RouteGroupBuilder MapLogoutFromAllDevices(this RouteGroupBuilder group)
    {
        group.MapPost("/logout/from-all", LogoutFromAllDevices)
             .WithName(nameof(LogoutFromAllDevices))
             .Produces<NoContent>(StatusCodes.Status204NoContent);

        return group;
    }

    private static async Task<IResult> LogoutFromAllDevices(
        HttpContext httpContext,
        ICommandHandler<LogoutFromAllDevicesCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var userIdResult = GetUserIdFromClaims(httpContext);
        if (userIdResult.IsFailure) return userIdResult.ProcessError();

        DeleteRefreshTokenCookie(httpContext);
        
        await commandHandler.HandleAsync(
            new LogoutFromAllDevicesCommand(userIdResult.Value),
            cancellationToken);
        
        return Results.NoContent();
        
    }
}