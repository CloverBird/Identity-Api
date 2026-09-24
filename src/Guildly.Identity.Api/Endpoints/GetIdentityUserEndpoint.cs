using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Mapper;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.GetIdentityUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints;

internal static class GetIdentityUserEndpoint
{
    public static RouteGroupBuilder MapGetIdentityUser(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetIdentityUser)
             .WithName(nameof(GetIdentityUser))
             .Produces<GetIdentityUserResponse>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetIdentityUser(
        HttpContext httpContext,
        IQueryHandler<GetIdentityUserQuery, GetIdentityUserQueryResult> queryHandler,
        CancellationToken cancellationToken)
    {
        var userIdResult = GetUserIdFromClaims(httpContext);
        if (userIdResult.IsFailure) return userIdResult.ProcessError();

        var result = await queryHandler.HandleAsync(
            new GetIdentityUserQuery(userIdResult.Value),
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError() 
            : Results.Ok(ApiMapper.ToGetIdentityUserResponse(result.Value));
    }
}