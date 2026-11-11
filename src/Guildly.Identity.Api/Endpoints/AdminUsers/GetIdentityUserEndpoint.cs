using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Mapper;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.AdminUsers;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class GetIdentityUserEndpoint
{
    public static RouteGroupBuilder MapGetIdentityUser(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetIdentityUser)
             .WithName(nameof(GetIdentityUser))
             .Produces<GetIdentityUserResponse>()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces<string>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> GetIdentityUser(
        Guid id,
        IQueryHandler<GetIdentityUserQuery, GetIdentityUserQueryResult> queryHandler,
        CancellationToken cancellationToken)
    {
        var result = await queryHandler.HandleAsync(new GetIdentityUserQuery(id), cancellationToken);
        return result.IsFailure ? result.ProcessError() : Results.Ok(ApiMapper.ToGetIdentityUserResponse(result.Value));
    }
}
