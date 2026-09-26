namespace Guildly.Common.Database.Configurations;

public class DatabaseConfiguration
{
    public string Server { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int Port { get; set; }
    
    public string ConnectionString => $"Server={Server};Database={Name};Port={Port};User Id={Username};Password={Password};";
}