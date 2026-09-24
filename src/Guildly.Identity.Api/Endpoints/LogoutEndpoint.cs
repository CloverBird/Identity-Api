using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Application.Commands.Logout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using static Guildly.Identity.Api.Services.HttpContextHelpers;

namespace Guildly.Identity.Api.Endpoints;

internal static class LogoutEndpoint
{
    public static RouteGroupBuilder MapLogout(this RouteGroupBuilder group)
    {
        group.MapPost("/logout", Logout)
             .WithName(nameof(Logout))
             .Produces<BadRequest>(StatusCodes.Status400BadRequest)
             .Produces<NoContent>(StatusCodes.Status204NoContent);
        
        return group;
    }

    private static async Task<IResult> Logout(
        HttpContext httpContext,
        ICommandHandler<LogoutCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var refreshTokenResult = GetRefreshTokenFromCookie(httpContext);
        if (refreshTokenResult.IsFailure) return refreshTokenResult.ProcessError();

        DeleteRefreshTokenCookie(httpContext);
        
        var result = await commandHandler.HandleAsync(
            new LogoutCommand(refreshTokenResult.Value),
            cancellationToken);
        if (result.IsFailure) return result.ProcessError();
        
        return Results.NoContent();
    } 
}