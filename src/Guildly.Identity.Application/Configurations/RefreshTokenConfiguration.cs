namespace Guildly.Identity.Application.Configurations;

public class RefreshTokenConfiguration
{
    public string SecretKey { get; set; } = string.Empty;

    public int RefreshTokenLifeTime { get; set; } = 5;

    public int RememberMeRefreshTokenLifeTime { get; set; } = 1;

    public int Length { get; set; } = 32;
}