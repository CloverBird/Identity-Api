using Guildly.Identity.Application.Models;
using Guildly.Identity.Domain.Entities;

namespace Guildly.Identity.Application.Services;

public interface IAccessTokenProvider
{
    AccessToken GenerateAccessToken(IdentityUser user);
}