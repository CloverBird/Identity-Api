using Guildly.Common.CQRS;
using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Services;
using Guildly.Identity.Application.UnitOfWork;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;

namespace Guildly.Identity.Application.Commands.RegisterUser;

internal class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IVerificationSessionService _verificationSessionService;
    private readonly IIdentityUnitOfWork _identityUnitOfWork;
    
    private readonly IPasswordService _passwordService;

    public RegisterUserCommandHandler(IIdentityUserRepository identityUserRepository, 
                                      IVerificationSessionService verificationSessionService,
                                      IIdentityUnitOfWork identityUnitOfWork,
                                      IPasswordService passwordService)
    {
        _identityUserRepository = identityUserRepository;
        _verificationSessionService = verificationSessionService;
        _identityUnitOfWork = identityUnitOfWork;
        
        _passwordService = passwordService;
    }

    //TODO: user creation in Users module and Identity module should be in the same db transaction
    public async Task<Result> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = Guid.NewGuid();
        
        var passwordHash = _passwordService.HashPassword(command.Password);
        
        var userResult = IdentityUser.Create(userId, command.Email, passwordHash, UserRole.User, now); // TODO: think about roles
        if (userResult.IsFailure) return userResult.Error;
        var user = userResult.Value;

        var userWithEmailExists = await _identityUserRepository.ExistsByEmailAsync(user.Email, cancellationToken);
        if (userWithEmailExists) return IdentityApplicationErrors.UserWithEmailAlreadyExists;
        
        var startSessionResult = await _verificationSessionService.StartVerificationSessionAsync(
            user.Id,
            user.Email.Value,
            VerificationPurpose.RegisterEmail,
            VerificationChannel.Email,
            now,
            cancellationToken);
        if (startSessionResult.IsFailure) return startSessionResult.Error;
        
        // TODO: send notification with code
        var code = startSessionResult.Value.Code;
        
        // NOTE: this method should create user in user api
        // var userCreatingResult = await _usersModuleService.CreateUser(
        //     new CreateUserRequest(userId, command.Username, command.DisplayName, now.UtcDateTime), 
        //     cancellationToken);
        // if (userCreatingResult.IsFailure) return  userCreatingResult.Error;
        
        _identityUserRepository.Add(user);

        await _identityUnitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success;
    }
}