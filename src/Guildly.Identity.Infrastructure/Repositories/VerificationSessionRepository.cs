using Guildly.Common.Database.Repository;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Guildly.Identity.Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Guildly.Identity.Infrastructure.Repositories;

internal class VerificationSessionRepository : Repository<VerificationSession, IdentityDbContext>, IVerificationSessionRepository
{
    public VerificationSessionRepository(IdentityDbContext context) 
        : base(context) { }

    public Task<VerificationSession?> GetLastVerificationSession(
        Guid userId, 
        VerificationPurpose? purpose = null, 
        VerificationChannel? channel = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.VerificationSessions.AsNoTracking() : DbContext.VerificationSessions;
        
        return query.Where(s => 
                s.UserId == userId
                && (purpose == null || s.Purpose == purpose)
                && (channel == null || s.Channel == channel))
            .OrderByDescending(u => u.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<List<VerificationSession>> GetActiveVerificationSessions(
        Guid userId, 
        DateTimeOffset now,
        VerificationPurpose? purpose = null,
        VerificationChannel? channel = null, 
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = asNoTracking ? DbContext.VerificationSessions.AsNoTracking() : DbContext.VerificationSessions;
        
        return query.Where(s =>
                s.UserId == userId
                && s.CancelledAt == null && s.UsedAt == null && s.ExpiresAt > now
                && (purpose == null || s.Purpose == purpose)
                && (channel == null || s.Channel == channel))
            .OrderByDescending(u => u.CreatedAt).ToListAsync(cancellationToken);
    }
}