using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Mapper;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.AdminUsers;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class GetIdentityUsersEndpoint
{
    public static RouteGroupBuilder MapGetIdentityUsers(this RouteGroupBuilder group)
    {
        group.MapGet("", GetIdentityUsers)
             .WithName(nameof(GetIdentityUsers))
             .Produces<GetIdentityUsersResponse>()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetIdentityUsers(
        [AsParameters] GetIdentityUsersRequest request,
        IQueryHandler<GetIdentityUsersQuery, GetIdentityUsersQueryResult> queryHandler,
        CancellationToken cancellationToken)
    {
        var page = request.Page ?? 1;
        var pageSize = request.PageSize ?? 20;
        if (page < 1 || pageSize is < 1 or > 100)
        {
            return Results.BadRequest("Page must be at least 1 and pageSize must be between 1 and 100.");
        }

        var skip = ((long)page - 1) * pageSize;
        if (skip > int.MaxValue)
        {
            return Results.BadRequest("The requested page is too large.");
        }

        var result = await queryHandler.HandleAsync(new GetIdentityUsersQuery((int)skip, pageSize), cancellationToken);
        if (result.IsFailure)
        {
            return result.ProcessError();
        }

        return Results.Ok(new GetIdentityUsersResponse(
            result.Value.Items.Select(ApiMapper.ToGetIdentityUserResponse).ToArray(),
            page,
            pageSize,
            result.Value.TotalCount));
    }
}
