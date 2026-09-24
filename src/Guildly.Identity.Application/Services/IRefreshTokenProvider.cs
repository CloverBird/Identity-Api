using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Application.Services;

public interface IRefreshTokenProvider
{ 
    string GenerateRefreshToken(IdentityUser user);

    string HashRefreshToken(string token);
}