using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Commands.AdminUsers.BlockIdentityUser;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class BlockIdentityUserEndpoint
{
    public static RouteGroupBuilder MapBlockIdentityUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/block", BlockIdentityUser)
             .WithName(nameof(BlockIdentityUser))
             .Produces<IdentityUserStatusResponse>()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces<string>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> BlockIdentityUser(
        Guid id,
        ICommandHandler<BlockIdentityUserCommand, UserStatus> commandHandler,
        CancellationToken cancellationToken)
    {
        var result = await commandHandler.HandleAsync(new BlockIdentityUserCommand(id), cancellationToken);
        return result.IsFailure
            ? result.ProcessError()
            : Results.Ok(new IdentityUserStatusResponse(id, result.Value));
    }
}
