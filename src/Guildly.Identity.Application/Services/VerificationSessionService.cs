using Guildly.Common.Application;
using Guildly.Common.Results;
using Guildly.Identity.Application.Configurations;
using Guildly.Identity.Application.Errors;
using Guildly.Identity.Application.Models;
using Guildly.Identity.Domain.Entities;
using Guildly.Identity.Domain.Enums;
using Guildly.Identity.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Guildly.Identity.Application.Services;

internal class VerificationSessionService : IVerificationSessionService
{
    private readonly IVerificationSessionRepository _verificationSessionRepository;
    private readonly IVerificationCodeService _verificationCodeService;
    private readonly VerificationSessionConfiguration _verificationSessionConfiguration;

    public VerificationSessionService(IVerificationSessionRepository verificationSessionRepository, 
                                      IVerificationCodeService verificationCodeService, 
                                      IOptions<VerificationSessionConfiguration> options)
    {
        _verificationSessionRepository = verificationSessionRepository;
        _verificationCodeService = verificationCodeService;
        _verificationSessionConfiguration = options.Value;
    }

    public async Task<Result<StartVerificationSessionResult>> StartVerificationSessionAsync(
        Guid userId, 
        string sentTo, 
        VerificationPurpose purpose, 
        VerificationChannel channel,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        // cancel previous sessions
        await CancelActiveVerificationSessionsAsync(
            userId,
            purpose,
            createdAt,
            cancellationToken);
        
        var code = _verificationCodeService.GenerateVerificationCode();
        
        var verificationSessionResult = VerificationSession.Create(
            userId, 
            sentTo,
            purpose, 
            channel, 
            _verificationCodeService.HashCode(code),
            createdAt.AddMinutes(_verificationSessionConfiguration.ExpirationTimeInMinutes),
            createdAt);
        if (verificationSessionResult.IsFailure)
        {
            return verificationSessionResult.Error;
        }
        _verificationSessionRepository.Add(verificationSessionResult.Value);

        return new StartVerificationSessionResult(code);
    }
    
    public async Task<Result<VerificationSession>> ConfirmSessionAsync(
        Guid userId, 
        string code,
        DateTimeOffset confirmedAt,
        VerificationPurpose? purpose = null, 
        VerificationChannel? channel = null,
        CancellationToken cancellationToken = default)
    {
        var session = await _verificationSessionRepository.GetLastVerificationSession(
            userId, 
            purpose,
            channel,
            false,
            cancellationToken);
        if (session is null)
        {
            return IdentityApplicationErrors.VerificationSessionNotFound;
        }
        
        var sessionCanBeConfirmedResult = session.EnsureCodeCanBeConfirmed(
            confirmedAt, 
            _verificationSessionConfiguration.AllowedAttempts);
        if (sessionCanBeConfirmedResult.IsFailure)
        {
            return sessionCanBeConfirmedResult.Error;
        }
        
        var result = _verificationCodeService.VerifyCode(code, session.CodeHash);

        if (!result)
        {
            session.RegisterFailedAttempt();
            
            return IdentityApplicationErrors.InvalidConfirmationCode;
        } 
        
        session.MarkUsed(confirmedAt);

        return session;
    }
    
    public async Task CancelActiveVerificationSessionsAsync(
        Guid userId, 
        VerificationPurpose purpose, 
        DateTimeOffset cancelledAt,
        CancellationToken cancellationToken = default)
    {
        var activeSessions = await _verificationSessionRepository.GetActiveVerificationSessions(
            userId,
            cancelledAt,
            purpose,
            null,
            false,
            cancellationToken);
        foreach (var verificationSession in activeSessions)
        {
            verificationSession.Cancel(cancelledAt);
        }
    }
}