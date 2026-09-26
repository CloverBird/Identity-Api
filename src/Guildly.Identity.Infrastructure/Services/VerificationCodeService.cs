using System.Security.Cryptography;
using System.Text;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace Guildly.Identity.Infrastructure.Services;

public class VerificationCodeService : IVerificationCodeService
{
    private const string AllowedSymbols = "abcdefghijklmnopqrstuvwxy0123456789";
    private readonly int _length;

    private readonly byte[] _secretKey;

    public VerificationCodeService(IOptions<VerificationCodeConfiguration> verificationCodeConfiguration)
    {
        _length = verificationCodeConfiguration.Value.Length;
        _secretKey = Encoding.UTF8.GetBytes(verificationCodeConfiguration.Value.SecretKey);
    }

    public string GenerateVerificationCode()
    {
        var result = new char[_length];
        for (var i = 0; i < _length; i++)
        {
            result[i] = AllowedSymbols[RandomNumberGenerator.GetInt32(AllowedSymbols.Length)];
        }
        
        return new string(result);
    }

    public string HashCode(string code)
    {
        using var hmac = new HMACSHA256(_secretKey);

        var data = Encoding.UTF8.GetBytes(code);

        var hash = hmac.ComputeHash(data);

        return Convert.ToHexString(hash);
    }

    public bool VerifyCode(string code, string codeHash)
    {
        var actualHash = HashCode(code);

        return CryptographicOperations.FixedTimeEquals(
            Convert.FromHexString(actualHash),
            Convert.FromHexString(codeHash));
    }
}