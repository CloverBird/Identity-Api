using FluentValidation;
using Guildly.Common.Api.Extensions;
using Guildly.Common.CQRS;
using Guildly.Identity.Api.Requests;
using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Commands.Login;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using static Guildly.Identity.Api.Services.HttpContextHelpers;

namespace Guildly.Identity.Api.Endpoints;

internal static class LoginEndpoint
{
    public static RouteGroupBuilder MapLogin(this RouteGroupBuilder group)
    {
        group.MapPost("/login", Login)
             .WithName(nameof(LoginEndpoint))
             .Produces<AccessTokenResponse>()
             .ProducesValidationProblem()
             .Produces(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status401Unauthorized)
             .Produces(StatusCodes.Status403Forbidden)  // if account is blocked
             .AllowAnonymous();

        return group;
    }
    
    private static async Task<IResult> Login(
        LoginUserRequest request,
        IValidator<LoginUserRequest> validator,
        HttpContext httpContext,
        ICommandHandler<LoginUserCommand, LoginUserCommandResult> commandHandler,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        var result = await commandHandler.HandleAsync(
            new LoginUserCommand(request.Email, request.Password, request.RememberMe),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ProcessError();
        }
        
        AddRefreshTokenCookie(
            httpContext, 
            result.Value.RefreshToken, 
            result.Value.RefreshTokenExpiresAt);
        
        return Results.Ok(new AccessTokenResponse(result.Value.AccessToken, result.Value.AccessTokenExpiresAt));
    }
}