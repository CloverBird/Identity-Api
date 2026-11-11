using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Commands.AdminUsers.UnblockIdentityUser;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class UnblockIdentityUserEndpoint
{
    public static RouteGroupBuilder MapUnblockIdentityUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/unblock", UnblockIdentityUser)
             .WithName(nameof(UnblockIdentityUser))
             .Produces<IdentityUserStatusResponse>()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces<string>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> UnblockIdentityUser(
        Guid id,
        ICommandHandler<UnblockIdentityUserCommand, UserStatus> commandHandler,
        CancellationToken cancellationToken)
    {
        var result = await commandHandler.HandleAsync(new UnblockIdentityUserCommand(id), cancellationToken);
        return result.IsFailure
            ? result.ProcessError()
            : Results.Ok(new IdentityUserStatusResponse(id, result.Value));
    }
}
