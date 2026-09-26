using System.Security.Claims;
using Guildly.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Guildly.Common.Api.Services;

public static class HttpContextHelpers
{
    private const string AnonymousUserIdClaim = "anonymousUserId";
    
    public static Result<Guid> GetUserIdFromClaims(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) 
            ? id 
            : Error.Unauthorized("identity.cannot_get_userId_from_claims", $"Could not parse claim {claim} as a Guid");
    }

    // Return userId if user is registered and anonymousUserId if not
    public static Result<(bool IsRegistered, Guid UserId)> GetUserOrAnonymousUserIdFromClaims(HttpContext httpContext)
    {
        var claim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(claim, out var userId))
        {
            return (true, userId);
        }
        
        var anonymousUserIdClaim = httpContext.User.FindFirstValue(AnonymousUserIdClaim);
        if (Guid.TryParse(anonymousUserIdClaim, out var anonymousUserId))
        {
            return (false, anonymousUserId);
        }

        return Error.Unauthorized("identity.cannot_get_userId_from_claims", $"Could not parse claim {claim} as a Guid");
    }
}