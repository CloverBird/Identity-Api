namespace Guildly.Identity.Infrastructure.Configurations;

public class JwtConfiguration
{
    public string SecretKey { get; set; } = string.Empty;
    
    public string Issuer { get; set; } = string.Empty;
    
    public string Audience { get; set; } = string.Empty;

    public int AccessTokenLifetimeInMinutes { get; set; } = 10;
}