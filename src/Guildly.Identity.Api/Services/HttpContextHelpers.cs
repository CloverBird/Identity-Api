using Guildly.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Guildly.Identity.Api.Services;

internal static class HttpContextHelpers
{
    public static void AddRefreshTokenCookie(HttpContext httpContext, string refreshToken, DateTimeOffset expiresAt)
    {
        httpContext.Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // NOTE: this should be set to true in production
                Expires = expiresAt,
                SameSite = SameSiteMode.Strict,
                Path = "/api/auth"
            });
    }

    public static void DeleteRefreshTokenCookie(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            "refreshToken",
            new CookieOptions
            {
                Path = "/api/auth"
            });
    }

    public static Result<string> GetRefreshTokenFromCookie(HttpContext httpContext)
    {
        return httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken) 
            ? refreshToken
            : Error.Unauthorized("identity.invalid_refresh_token", "Invalid refresh token");
    }
}