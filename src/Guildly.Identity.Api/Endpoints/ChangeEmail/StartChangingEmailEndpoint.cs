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

internal static class StartChangingEmailEndpoint
{
    public static RouteGroupBuilder MapStartChangingEmail(this RouteGroupBuilder group)
    {
        group.MapPost("/email/change/start", StartChangingEmail)
             .WithName(nameof(StartChangingEmail))
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces<BadRequest>(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound)
             .Produces<Conflict>(StatusCodes.Status409Conflict);
        
        return group;
    }

    private static async Task<IResult> StartChangingEmail(
        HttpContext httpContext,
        StartChangingEmailRequest request,
        IValidator<StartChangingEmailRequest> validator,
        ICommandHandler<StartChangingEmailCommand> commandHandler,
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
            new StartChangingEmailCommand(userIdResult.Value, request.Password, request.NewEmail),
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError() 
            : Results.NoContent();
    }
}