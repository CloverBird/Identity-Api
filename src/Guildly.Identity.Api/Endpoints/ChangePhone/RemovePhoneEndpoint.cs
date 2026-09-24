using FluentValidation;
using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.RemovePhone;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.ChangePhone;

internal static class RemovePhoneEndpoint
{
    public static RouteGroupBuilder MapRemovePhone(this RouteGroupBuilder group)
    {
        group.MapDelete("/phone/remove", RemovePhone)
            .WithName(nameof(RemovePhone))
            .ProducesValidationProblem()
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<NotFound>(StatusCodes.Status404NotFound);
        
        return group;
    }

    private static async Task<IResult> RemovePhone(
        HttpContext httpContext,
        [FromBody] RemovePhoneRequest request,
        IValidator<RemovePhoneRequest> validator,
        ICommandHandler<RemovePhoneCommand> commandHandler,
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
            new RemovePhoneCommand(userIdResult.Value, request.Password),
            cancellationToken);
        
        return result.IsFailure ? result.ProcessError() : Results.NoContent();
    }
}