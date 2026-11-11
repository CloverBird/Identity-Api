using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Commands.AdminUsers.CreateIdentityUser;

namespace Guildly.Identity.Api.Endpoints.AdminUsers;

internal static class CreateIdentityUserEndpoint
{
    public static RouteGroupBuilder MapCreateIdentityUser(this RouteGroupBuilder group)
    {
        group.MapPost("", CreateIdentityUser)
             .WithName(nameof(CreateIdentityUser))
             .Produces<GetIdentityUserResponse>(StatusCodes.Status201Created)
             .ProducesValidationProblem()
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)
             .Produces<string>(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> CreateIdentityUser(
        CreateIdentityUserRequest request,
        IValidator<CreateIdentityUserRequest> validator,
        ICommandHandler<CreateIdentityUserCommand, CreateIdentityUserCommandResult> commandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new CreateIdentityUserCommand(request.Email, request.Password), cancellationToken);
        if (result.IsFailure)
        {
            return result.ProcessError();
        }

        var user = result.Value;
        var response = new GetIdentityUserResponse(
            user.Id, user.Email, user.Status, user.EmailConfirmed, user.CreatedAt);

        return Results.CreatedAtRoute(
            routeName: "GetIdentityUser",
            routeValues: new { id = user.Id },
            value: response);
    }
}
