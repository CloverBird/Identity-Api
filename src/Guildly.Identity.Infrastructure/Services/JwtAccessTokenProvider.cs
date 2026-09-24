using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Guildly.Identity.Infrastructure.Services;

public class JwtAccessTokenProvider : IAccessTokenProvider
{
    private readonly JwtConfiguration _jwtConfiguration;

    public JwtAccessTokenProvider(IOptions<JwtConfiguration> jwtConfiguration)
    {
        ArgumentNullException.ThrowIfNull(jwtConfiguration);

        _jwtConfiguration = jwtConfiguration.Value;
    }
    
    public AccessToken GenerateAccessToken(IdentityUser user)
    {
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(_jwtConfiguration.AccessTokenLifetimeInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.SecretKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtConfiguration.Issuer,
            audience: _jwtConfiguration.Audience,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            claims: claims,
            signingCredentials: signingCredentials);
        
        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        
        return new AccessToken(tokenValue, expiresAt);
    }
}