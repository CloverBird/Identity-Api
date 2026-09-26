using FluentValidation;
using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.ChangePhone;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.ChangePhone;

internal static class ConfirmChangingPhoneEndpoint
{
    public static RouteGroupBuilder MapConfirmChangingPhone(this RouteGroupBuilder group)
    {
        group.MapPost("/phone/change/confirm", ConfirmChangingPhone)
             .WithName($"{nameof(ConfirmChangingPhone)}")
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound)
             .Produces<Conflict>(StatusCodes.Status409Conflict);
        
        return group;
    }

    private static async Task<IResult> ConfirmChangingPhone(
        HttpContext httpContext,
        ConfirmCodeRequest request,
        IValidator<ConfirmCodeRequest> validator,
        ICommandHandler<ConfirmChangingPhoneCommand> commandHandler,
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
            new ConfirmChangingPhoneCommand(userIdResult.Value, request.Code), 
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError()
            : Results.NoContent();
    }
}