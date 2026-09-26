using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.GetIdentityUser;

namespace Guildly.Identity.Api.Mapper;

public static class ApiMapper
{
    public static GetIdentityUserResponse ToGetIdentityUserResponse(GetIdentityUserQueryResult identityUser)
    {
        return new GetIdentityUserResponse(
            identityUser.UserId,
            identityUser.Email,
            identityUser.Phone,
            identityUser.CreatedAt);
    }
}