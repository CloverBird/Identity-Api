using Guildly.Common.Application;
using Guildly.Common.Results;
using Guildly.Identity.Domain.ValueObjects;

namespace Guildly.Identity.Application.Services;

internal interface IIdentityContactsService
{
    // when succeed returns unique and valid Email
    Task<Result<Email>> GetAvailableEmailAsync(string email, CancellationToken cancellationToken = default);
    
    // when succeed returns unique and valid Phone  
    Task<Result<Phone>> GetAvailablePhoneAsync(string phone, CancellationToken cancellationToken = default);
}