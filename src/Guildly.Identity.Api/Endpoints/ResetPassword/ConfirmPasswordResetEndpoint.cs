using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.ResetPassword;
using Guildly.Identity.Application.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.ResetPassword;

internal static class ConfirmPasswordResetEndpoint
{
    public static RouteGroupBuilder MapConfirmPasswordReset(this RouteGroupBuilder group)
    {
        group.MapPost("/password/reset/confirm", ConfirmPasswordReset)
            .WithName(nameof(ConfirmPasswordReset))
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<BadRequest>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> ConfirmPasswordReset(
        ConfirmPasswordResetRequest request,
        IValidator<ConfirmPasswordResetRequest> validator,
        ICommandHandler<ConfirmPasswordResetCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new ConfirmPasswordResetCommand(request.Target, request.Channel, request.NewPassword, request.Code),
            cancellationToken);
        
        if (result.IsFailure && 
            result.Error != IdentityApplicationErrors.UserNotFound &&
            result.Error != IdentityApplicationErrors.UserNotActive)
        {
            return result.ProcessError();
        }
        
        return Results.NoContent();
    }
}