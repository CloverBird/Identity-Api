using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;

namespace Guildly.Identity.Application.Commands.RemovePhone;

internal class RemovePhoneCommandHandler : ICommandHandler<RemovePhoneCommand>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;

    public RemovePhoneCommandHandler(IIdentityUserService identityUserService,
                                     IIdentityUnitOfWork identityUnitOfWork,
                                     IPasswordService passwordService)
    {
        _identityUserService = identityUserService;
        _identityUnitOfWork = identityUnitOfWork;
        _passwordService = passwordService;
    }

    public async Task<Result> HandleAsync(RemovePhoneCommand command, CancellationToken cancellationToken = default)
    {
        var userResult = await _identityUserService.GetActiveUserByIdAsync(command.UserId, cancellationToken);
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var passwordVerified = _passwordService.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordVerified) return IdentityApplicationErrors.InvalidPassword;

        var result = user.RemovePhone();
        if (result.IsFailure) return result.Error;
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}