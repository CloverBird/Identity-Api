using Guildly.Common.Results;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Services;

internal class IdentityContactsService : IIdentityContactsService
{
    private readonly IIdentityUserRepository _identityUserRepository;

    public IdentityContactsService(IIdentityUserRepository identityUserRepository)
    {
        _identityUserRepository = identityUserRepository;
    }

    public async Task<Result<Email>> GetAvailableEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return emailResult.Error;
        }
        
        var emailExists = await _identityUserRepository.ExistsByEmailAsync(emailResult.Value, cancellationToken);

        return emailExists ? IdentityApplicationErrors.UserWithEmailAlreadyExists : emailResult;
    }

    public async Task<Result<Phone>> GetAvailablePhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var phoneResult = Phone.Create(phone);
        if (phoneResult.IsFailure)
        {
            return phoneResult.Error;
        }
        
        var phoneExists = await _identityUserRepository.ExistsByPhoneAsync(phoneResult.Value, cancellationToken);

        return phoneExists ? IdentityApplicationErrors.UserWithPhoneAlreadyExists : phoneResult;
    }
}