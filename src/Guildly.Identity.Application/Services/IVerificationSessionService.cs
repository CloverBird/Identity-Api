using Guildly.Common.Results;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Application.Services;

internal interface IVerificationSessionService
{
    Task CancelActiveVerificationSessionsAsync(
        Guid userId,
        VerificationPurpose purpose,
        DateTimeOffset cancelledAt,
        CancellationToken cancellationToken = default);
    
    Task<Result<StartVerificationSessionResult>> StartVerificationSessionAsync(
        Guid userId,
        string sentTo,
        VerificationPurpose purpose,
        VerificationChannel channel,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Confirms the latest verification session for the specified user by checking the provided confirmation code.
    /// </summary>
    /// <remarks>
    /// This method mutates the loaded verification session, but it does not save changes.
    /// The caller is responsible for calling the appropriate unit of work after completing
    /// all related business changes.
    /// </remarks>
    Task<Result<VerificationSession>> ConfirmSessionAsync(
        Guid userId,
        string code,
        DateTimeOffset confirmedAt,
        VerificationPurpose? purpose = null,
        VerificationChannel? channel = null,
        CancellationToken cancellationToken = default);
}