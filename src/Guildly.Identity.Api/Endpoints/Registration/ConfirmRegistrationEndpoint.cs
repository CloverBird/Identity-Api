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

internal static class ConfirmRegistrationEndpoint
{
    public static RouteGroupBuilder MapConfirmRegistration(this RouteGroupBuilder group)
    {
        group.MapPost("/register/confirm-email", ConfirmRegistration)
             .WithName($"{nameof(ConfirmRegistration)}")
             .Produces<NoContent>(StatusCodes.Status204NoContent)
             .ProducesValidationProblem()
             .Produces<BadRequest>(StatusCodes.Status400BadRequest)
             .Produces<NotFound>(StatusCodes.Status404NotFound)
             .Produces<Conflict>(StatusCodes.Status409Conflict)
             .AllowAnonymous();
         
        return group;
    }
    
    private static async Task<IResult> ConfirmRegistration(
        ConfirmRegistrationRequest request,
        IValidator<ConfirmRegistrationRequest> validator,
        ICommandHandler<ConfirmRegistrationCommand> confirmRegistrationCommandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }
        
        var registerUserResult = await confirmRegistrationCommandHandler.HandleAsync(
            new ConfirmRegistrationCommand(request.Email, request.Code), 
            cancellationToken);
        
        return registerUserResult.IsFailure ? 
            registerUserResult.ProcessError() :
            Results.NoContent();
    }
}