namespace Guildly.Common.Results;

public enum ErrorType
{
    None = 0,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict
}