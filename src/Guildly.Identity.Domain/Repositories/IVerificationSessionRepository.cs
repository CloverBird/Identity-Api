using Guildly.Common.Domain;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;

namespace Guildly.Identity.Domain.Repositories;

public interface IVerificationSessionRepository : IRepository<VerificationSession>
{
    Task<VerificationSession?> GetLastVerificationSession(
        Guid userId,
        VerificationPurpose? purpose = null,
        VerificationChannel? channel = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);

    Task<List<VerificationSession>> GetActiveVerificationSessions(
        Guid userId,
        DateTimeOffset now,
        VerificationPurpose? purpose = null,
        VerificationChannel? channel = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default);
}