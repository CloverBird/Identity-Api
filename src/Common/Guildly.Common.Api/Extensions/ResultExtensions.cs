using Guildly.Common.Results;
using Microsoft.AspNetCore.Http;

namespace Guildly.Common.Api.Extensions;

//TODO: create better solution
// this is useless, because every endpoint should return all results from this method,
// and TypedResults loose its meaning fully, because every endpoint can return anything. 
// Trade off: endpoints return IResult and we should manually set produced responses to every endpoint.
public static class ResultExtensions
{
    public static IResult ProcessError(this Result result)
        => ProcessError(result.Error);

    public static IResult ProcessError<TValue>(this Result<TValue> result)
        => ProcessError(result.Error);

    private static IResult ProcessError(Error error)
    {
        return error.ErrorType switch
        {
            ErrorType.Validation => Microsoft.AspNetCore.Http.Results.BadRequest(error.Message),
            ErrorType.NotFound => Microsoft.AspNetCore.Http.Results.NotFound(error.Message),
            ErrorType.Conflict => Microsoft.AspNetCore.Http.Results.Conflict(error.Message),
            ErrorType.Forbidden => Microsoft.AspNetCore.Http.Results.Forbid(),
            ErrorType.Unauthorized => Microsoft.AspNetCore.Http.Results.Unauthorized(),
            ErrorType.None => throw new ArgumentException("Cannot process valid result"),
            _ => throw new ArgumentException("Unknown error type")
        }; 
    }
}