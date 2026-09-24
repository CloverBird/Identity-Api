namespace Guildly.Common.Results;

public readonly record struct Error(string Code, string Message, ErrorType ErrorType)
{
    public static Error None => new Error(string.Empty, string.Empty, ErrorType.None);

    public static Error Validation(string code, string message) => new Error(code, message, ErrorType.Validation);
    
    public static Error NotFound(string code, string message) => new Error(code, message, ErrorType.NotFound);
    
    public static Error Unauthorized(string code, string message) => new Error(code, message, ErrorType.Unauthorized);
    
    public static Error Forbidden(string code, string message) => new Error(code, message, ErrorType.Forbidden);

    public static Error Conflict(string code, string message) => new Error(code, message, ErrorType.Conflict);
}