namespace Guildly.Identity.Application.Configurations;

public class VerificationSessionConfiguration
{
    public int AllowedAttempts { get; set; } = 5;
    
    public int ExpirationTimeInMinutes { get; set; } = 5;
}