using FluentValidation;
using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.ChangeEmail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.ChangeEmail;

internal static class ConfirmChangingEmailEndpoint
{
    public static RouteGroupBuilder MapConfirmChangingEmail(this RouteGroupBuilder group)
    {
        group.MapPost("/email/change/confirm", ConfirmChangingEmail)
             .WithName($"{nameof(ConfirmChangingEmail)}")
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound)
             .Produces<Conflict>(StatusCodes.Status409Conflict);
        
        return group;
    }

    private static async Task<IResult> ConfirmChangingEmail(
        HttpContext httpContext,
        ConfirmCodeRequest request,
        IValidator<ConfirmCodeRequest> validator,
        ICommandHandler<ConfirmChangingEmailCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var userIdResult = GetUserIdFromClaims(httpContext);
        if (userIdResult.IsFailure) return userIdResult.ProcessError();
        
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new ConfirmChangingEmailCommand(userIdResult.Value, request.Code), 
            cancellationToken);
        if (result.IsFailure)
        {
            return result.ProcessError();
        }
        
        return Results.NoContent();
    }
}