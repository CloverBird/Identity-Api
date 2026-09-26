using FluentValidation;
using Guildly.Common.Api.Extensions;
using static Guildly.Common.Api.Services.HttpContextHelpers;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.ChangePassword;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints;

internal static class ChangePasswordEndpoint
{
    public static RouteGroupBuilder MapChangePassword(this RouteGroupBuilder group)
    {
        group.MapPost("/password/change", ChangePassword)
             .WithName(nameof(ChangePassword))
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces<BadRequest>(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<IResult> ChangePassword(
        HttpContext httpContext,
        ChangePasswordRequest request,
        IValidator<ChangePasswordRequest> validator,
        ICommandHandler<ChangePasswordCommand> commandHandler,
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
            new ChangePasswordCommand(userIdResult.Value, request.NewPassword, request.OldPassword),
            cancellationToken);
        
        return result.IsFailure 
            ? result.ProcessError() 
            : Results.NoContent();
    }
}