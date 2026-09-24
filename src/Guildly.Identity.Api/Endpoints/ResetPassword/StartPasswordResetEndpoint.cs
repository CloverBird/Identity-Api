using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.ResetPassword;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Domain.Errors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.ResetPassword;

internal static class StartPasswordResetEndpoint
{
    public static RouteGroupBuilder MapStartPasswordReset(this RouteGroupBuilder group)
    {
        group.MapPost("password/reset/start", StartPasswordReset)
            .WithName(nameof(StartPasswordReset))
            .Produces<NoContent>(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces<BadRequest>(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
        
        return group;
    }

    private static async Task<IResult> StartPasswordReset(
        StartPasswordResetRequest request,
        IValidator<StartPasswordResetRequest> validator,
        ICommandHandler<StartPasswordResetCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new StartPasswordResetCommand(request.Target, request.Channel),
            cancellationToken);

        if (result.IsFailure && 
            result.Error != IdentityApplicationErrors.UserNotFound &&
            result.Error != IdentityApplicationErrors.UserNotActive &&
            result.Error != IdentityDomainErrors.UnsupportedVerificationChannel)
        {
            return result.ProcessError();
        }
        
        // should not return an error for security
        return Results.NoContent();
    }
}