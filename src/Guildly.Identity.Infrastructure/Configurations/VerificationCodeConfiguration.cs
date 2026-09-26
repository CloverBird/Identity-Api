namespace Guildly.Identity.Infrastructure.Configurations;

public class VerificationCodeConfiguration
{
    public int Length { get; set; } = 6;
    
    public string SecretKey { get; set; } = string.Empty;
    
}