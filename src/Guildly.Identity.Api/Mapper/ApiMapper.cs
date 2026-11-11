using Guildly.Identity.Api.Responses;
using Guildly.Identity.Application.Queries.AdminUsers;
using Guildly.Identity.Application.Queries.GetMeAsUser;

namespace Guildly.Identity.Api.Mapper;

public static class ApiMapper
{
    public static GetIdentityUserResponse ToGetIdentityUserResponse(GetIdentityUserQueryResult user)
    {
        return new GetIdentityUserResponse(
            user.Id,
            user.Email,
            user.Status,
            user.EmailConfirmed,
            user.CreatedAt);
    }

    public static GetMeAsUserResponse ToGetMeAsUserResponse(GetMeAsUserQueryResult identityUser)
    {
        return new GetMeAsUserResponse(
            identityUser.UserId,
            identityUser.Email,
            identityUser.Phone,
            identityUser.CreatedAt);
    }
}
