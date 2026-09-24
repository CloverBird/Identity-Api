using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace Guildly.Common.Api.Extensions;

public static class ValidationResultExtensions
{
    public static IResult ToValidationProblem(this ValidationResult validationResult)
    {
        return Microsoft.AspNetCore.Http.Results.ValidationProblem(
            validationResult.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key, 
                    group => group
                        .Select(e => e.ErrorMessage)
                        .ToArray()));
    }
}