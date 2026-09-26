using System.Security.Cryptography;
using System.Text;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Guildly.Identity.Infrastructure.Services;

public class RefreshTokenProvider : IRefreshTokenProvider
{
    private const string AllowedSymbols = "abcdefghijklmnopqrstuvwxy0123456789";
    private readonly int _length;
    private readonly byte[] _secretKey;

    public RefreshTokenProvider(IOptions<RefreshTokenConfiguration> refreshTokenConfiguration)
    {
        _secretKey = Encoding.UTF8.GetBytes(refreshTokenConfiguration.Value.SecretKey);
        _length = refreshTokenConfiguration.Value.Length;
    }
    
    public string GenerateRefreshToken(IdentityUser user)
    {
        var result = new char[_length];
        for (var i = 0; i < _length; i++)
        {
            result[i] = AllowedSymbols[RandomNumberGenerator.GetInt32(AllowedSymbols.Length)];
        }
        
        return new string(result);
    }

    public string HashRefreshToken(string token)
    {
        using var hmac = new HMACSHA256(_secretKey);

        var data = Encoding.UTF8.GetBytes(token);

        var hash = hmac.ComputeHash(data);

        return Convert.ToHexString(hash);
    }
}