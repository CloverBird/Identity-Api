using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Application.Commands.AdminUsers.DeleteIdentityUser;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class DeleteIdentityUserEndpoint
{
    public static RouteGroupBuilder MapDeleteIdentityUser(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", DeleteIdentityUser)
             .WithName(nameof(DeleteIdentityUser))
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces<string>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> DeleteIdentityUser(
        Guid id,
        ICommandHandler<DeleteIdentityUserCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var result = await commandHandler.HandleAsync(new DeleteIdentityUserCommand(id), cancellationToken);
        return result.IsFailure ? result.ProcessError() : Results.NoContent();
    }
}
