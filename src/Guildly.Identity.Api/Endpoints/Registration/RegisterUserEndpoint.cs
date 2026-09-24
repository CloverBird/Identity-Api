using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Application.Commands.RegisterUser;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Guildly.Identity.Api.Endpoints.Registration;

internal static class RegisterUserEndpoint
{
    public static RouteGroupBuilder MapRegisterUser(this RouteGroupBuilder group)
    {
        group.MapPost("/register", RegisterUser)
            .WithName($"{nameof(RegisterUser)}")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<string>(StatusCodes.Status409Conflict)
            .AllowAnonymous();
         
        return group;
    }
    
    private static async Task<IResult> RegisterUser(
        RegisterUserRequest request,
        IValidator<RegisterUserRequest> validator,
        ICommandHandler<RegisterUserCommand> registerUserCommandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }
        
        var registerUserResult = await registerUserCommandHandler.HandleAsync(
            new RegisterUserCommand(request.Email, request.Username, request.DisplayName, request.Password), 
            cancellationToken);
        
        return registerUserResult.IsFailure ? 
            registerUserResult.ProcessError() :
            Results.NoContent();
    }
}