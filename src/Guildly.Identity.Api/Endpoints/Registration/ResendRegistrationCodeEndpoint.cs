using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.RegisterUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.Registration;

internal static class ResendRegistrationCodeEndpoint
{
    public static RouteGroupBuilder MapResendRegistrationCode(this RouteGroupBuilder group)
    {
        group.MapPost("/register/resend-code", ResendRegistrationCode)
             .WithName(nameof(ResendRegistrationCode))
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces<BadRequest>(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound)
             .Produces<Conflict>(StatusCodes.Status409Conflict)
             .AllowAnonymous();

        return group;
    }

    private static async Task<IResult> ResendRegistrationCode(
        ResendRegistrationCodeRequest request,
        IValidator<ResendRegistrationCodeRequest> validator,
        ICommandHandler<ResendRegistrationCodeCommand> commandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }
        
        var result = await commandHandler.HandleAsync(
            new ResendRegistrationCodeCommand(request.Email),
            cancellationToken);
        
        return result.IsFailure ? result.ProcessError() : Results.NoContent();
    }
}