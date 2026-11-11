using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Mapper;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.GetMeAsUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints;

internal static class GetMeAsUserEndpoint
{
    public static RouteGroupBuilder MapGetMeAsUser(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetMeAsUser)
             .WithName(nameof(GetMeAsUser))
             .Produces<GetMeAsUserResponse>(StatusCodes.Status200OK)
             .Produces(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetMeAsUser(
        HttpContext httpContext,
        IQueryHandler<GetMeAsUserQuery, GetMeAsUserQueryResult> queryHandler,
        CancellationToken cancellationToken)
    {
        var userIdResult = GetUserIdFromClaims(httpContext);
        if (userIdResult.IsFailure) return userIdResult.ProcessError();

        var result = await queryHandler.HandleAsync(
            new GetMeAsUserQuery(userIdResult.Value),
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError() 
            : Results.Ok(ApiMapper.ToGetMeAsUserResponse(result.Value));
    }
}
