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

internal static class StartChangingPhoneEndpoint
{
    public static RouteGroupBuilder MapStartChangingPhone(this RouteGroupBuilder group)
    {
        group.MapPost("/phone/change/start", StartChangingPhone)
            .WithName(nameof(StartChangingPhone))
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<BadRequest>(StatusCodes.Status400BadRequest)
            .Produces<NotFound>(StatusCodes.Status404NotFound)
            .Produces<Conflict>(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> StartChangingPhone(
        HttpContext httpContext,
        StartChangingPhoneRequest request,
        IValidator<StartChangingPhoneRequest> validator,
        ICommandHandler<StartChangingPhoneCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var userIdResult = GetUserIdFromClaims(httpContext);
        if (userIdResult.IsFailure) return userIdResult.ProcessError();
        
        var validatorResult = validator.Validate(request);
        if (!validatorResult.IsValid)
        {
            return validatorResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new StartChangingPhoneCommand(userIdResult.Value, request.Password, request.NewPhone),
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError() 
            : Results.NoContent();
    }
}