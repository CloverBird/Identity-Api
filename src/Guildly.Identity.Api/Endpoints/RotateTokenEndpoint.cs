using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Commands.RotateToken;
using Microsoft.AspNetCore.Http.HttpResults;
using static Guildly.Identity.Api.Services.HttpContextHelpers;

namespace Guildly.Identity.Api.Endpoints;

internal static class RotateTokenEndpoint
{
    public static RouteGroupBuilder MapRotateToken(this RouteGroupBuilder group)
    {
        group.MapPost("/refresh/rotate", RotateToken)
            .WithName(nameof(RotateToken))
            .Produces<AccessTokenResponse>(StatusCodes.Status200OK)
            .Produces<BadRequest>(StatusCodes.Status400BadRequest)
            .Produces<NotFound>(StatusCodes.Status404NotFound)
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> RotateToken(
        HttpContext httpContext,
        ICommandHandler<RotateTokenCommand, RotateTokenCommandResult> commandHandler,
        CancellationToken cancellationToken)
    {
        var refreshTokenResult = GetRefreshTokenFromCookie(httpContext);
        if (refreshTokenResult.IsFailure) return refreshTokenResult.ProcessError();

        var result = await commandHandler.HandleAsync(
            new RotateTokenCommand(refreshTokenResult.Value),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ProcessError();
        }
        
        AddRefreshTokenCookie(
            httpContext, 
            result.Value.NewRefreshToken, 
            result.Value.NewRefreshTokenExpiresAt);
        
        return Results.Ok(new AccessTokenResponse(
            result.Value.NewAccessToken,
            result.Value.NewAccessTokenExpiresAt));
    }
}