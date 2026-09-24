using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Commands.Login;

internal class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserCommandResult>
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IRefreshTokenService _refreshTokenService;
    
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IAccessTokenProvider _accessTokenProvider;
    
    private readonly IPasswordService _passwordService;

    public LoginUserCommandHandler(IIdentityUserRepository identityUserRepository,
                                   IPasswordService passwordService,
                                   IAccessTokenProvider accessTokenProvider,
                                   IRefreshTokenService refreshTokenService, 
                                   IIdentityUnitOfWork identityUnitOfWork)
    {
        _identityUserRepository = identityUserRepository;
        _passwordService = passwordService;
        _accessTokenProvider = accessTokenProvider;
        _refreshTokenService = refreshTokenService;
        _identityUnitOfWork = identityUnitOfWork;
    }

    public async Task<Result<LoginUserCommandResult>> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return IdentityApplicationErrors.InvalidCredentials;
        }

        var user = await _identityUserRepository.GetUserByEmailAsync(emailResult.Value, true, cancellationToken);
        if (user is null)
        {
            return IdentityApplicationErrors.InvalidCredentials; 
        }

        var passwordVerified = _passwordService.VerifyPassword(command.Password, user.PasswordHash);
        if (!passwordVerified)
        {
            return IdentityApplicationErrors.InvalidCredentials;  
        }

        if (user.Status != UserStatus.Active)
        {
            return IdentityApplicationErrors.UserNotActive;
        }

        var accessToken = _accessTokenProvider.GenerateAccessToken(user);

        var refreshToken = _refreshTokenService.CreateRefreshToken(user, now, command.RememberMe);
        
        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return new LoginUserCommandResult(
            accessToken.Token, accessToken.ExpiresAt,
            refreshToken.Token, refreshToken.ExpiresAt);
    }
}